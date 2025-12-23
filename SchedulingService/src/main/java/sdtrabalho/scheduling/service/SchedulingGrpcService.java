package sdtrabalho.scheduling.service;

import sdtrabalho.scheduling.proto.*;
import io.grpc.stub.StreamObserver;
import org.springframework.stereotype.Service;
import sdtrabalho.scheduling.entity.AppointmentEntity;
import sdtrabalho.scheduling.repository.AppointmentRepository;

import java.time.LocalDateTime;
import java.util.*;
import java.util.stream.Collectors;

@Service
public class SchedulingGrpcService extends SchedulingServiceGrpc.SchedulingServiceImplBase {

    private final AppointmentRepository repository;

    public SchedulingGrpcService(AppointmentRepository repository) {
        this.repository = repository;
    }

    private Appointment toProto(AppointmentEntity entity) {
        return Appointment.newBuilder()
                .setId(String.valueOf(entity.getId()))
                .setPatientId(entity.getPatientId())
                .setDoctorId(entity.getDoctorId())
                .setSpecialty(entity.getSpecialty())
                .setDatetime(entity.getDatetime().toString())
                .setStatus(AppointmentStatus.valueOf(entity.getStatus()))
                .build();
    }

    private AppointmentEntity toEntity(CreateAppointmentRequest request) {
        AppointmentEntity e = new AppointmentEntity();
        e.setPatientId(request.getPatientId());
        e.setDoctorId(request.getDoctorId());
        e.setSpecialty(request.getSpecialty());
        e.setDatetime(LocalDateTime.parse(request.getDatetime()));
        e.setStatus(AppointmentStatus.STATUS_SCHEDULED.name());
        return e;
    }

    @Override
    public void createAppointment(CreateAppointmentRequest request,
                                  StreamObserver<CreateAppointmentResponse> responseObserver) {

        try {
            LocalDateTime start = LocalDateTime.parse(request.getDatetime());
            LocalDateTime end = start.plusMinutes(30);

            boolean freeSchedule =
                    repository.findByDatetimeBetweenAndStatusNot(
                            start,
                            end,
                            AppointmentStatus.STATUS_CANCELLED.name()
                    ).isEmpty();

            if (!freeSchedule)
            {
                responseObserver.onError(
                        new IllegalArgumentException("Já existe uma consulta nesse intervalo")
                );
                return;
            }

            AppointmentEntity entity = toEntity(request);
            entity.setStatus("STATUS_SCHEDULED");
            AppointmentEntity saved = repository.save(entity);

            CreateAppointmentResponse response = CreateAppointmentResponse.newBuilder()
                    .setAppointment(toProto(saved))
                    .build();

            responseObserver.onNext(response);
            responseObserver.onCompleted();
        } catch (Exception ex) {
            responseObserver.onError(
                    io.grpc.Status.INTERNAL
                            .withDescription(ex.getMessage())
                            .withCause(ex)
                            .asRuntimeException()
            );
        }
    }

    @Override
    public void listAppointments(ListAppointmentsRequest request,
                                 StreamObserver<ListAppointmentsResponse> responseObserver) {

        List<AppointmentEntity> list;
        if (request.getIsDoctor()) {
            list = repository.findByDoctorId(request.getUserId());
        } else {
            list = repository.findByPatientId(request.getUserId());
        }

        List<Appointment> protoList = list.stream()
                .map(this::toProto)
                .collect(Collectors.toList());

        ListAppointmentsResponse response = ListAppointmentsResponse.newBuilder()
                .addAllAppointments(protoList)
                .build();

        responseObserver.onNext(response);
        responseObserver.onCompleted();
    }

    @Override
    public void updateStatus(UpdateStatusRequest request,
                             StreamObserver<UpdateStatusResponse> responseObserver) {

        Long id = Long.valueOf(request.getAppointmentId());

        AppointmentEntity entity = repository.findById(id).orElse(null);
        if (entity == null) {
            responseObserver.onError(
                    new IllegalArgumentException("Consulta não encontrada: " + request.getAppointmentId())
            );
            return;
        }

        entity.setStatus(request.getNewStatus().name());
        AppointmentEntity updated = repository.save(entity);

        UpdateStatusResponse response = UpdateStatusResponse.newBuilder()
                .setAppointment(toProto(updated))
                .build();

        responseObserver.onNext(response);
        responseObserver.onCompleted();
    }
}