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
                .setId(entity.getId())
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

            if (start.isBefore(LocalDateTime.now())){
                CreateAppointmentResponse resp = CreateAppointmentResponse.newBuilder()
                    .setStatus(false)
                    .setMensage("A data da consulta deve ser maior do que a data atual")
                    .build();

                responseObserver.onNext(resp);
                responseObserver.onCompleted();
                return;
            }

            boolean freeSchedule =
                    repository.findAppointmentConflicts(
                            start,
                            AppointmentStatus.STATUS_CANCELLED.name(),
                            request.getDoctorId(),
                            request.getPatientId()

                    ).isEmpty();

            if (!freeSchedule){
                CreateAppointmentResponse resp = CreateAppointmentResponse.newBuilder()
                    .setStatus(false)
                    .setMensage("Ja existe uma consulta nesse intervalo")
                    .build();

                responseObserver.onNext(resp);
                responseObserver.onCompleted();
                return;
            }

            AppointmentEntity entity = toEntity(request);
            entity.setStatus("STATUS_SCHEDULED");
            AppointmentEntity saved = repository.save(entity);

            CreateAppointmentResponse resp = CreateAppointmentResponse.newBuilder()
                .setStatus(true)
                .setMensage("Agendamento criado com sucesso")
                .setDados(toProto(saved))
                .build();

            responseObserver.onNext(resp);
            responseObserver.onCompleted();
        } catch (Exception ex) {
            CreateAppointmentResponse resp = CreateAppointmentResponse.newBuilder()
                .setStatus(false)
                .setMensage(ex.getMessage())
                .build();

            responseObserver.onNext(resp);
            responseObserver.onCompleted();
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
                .addAllDados(protoList)
                .setMensage("Agendamentos carregado com sucesso")
                .setStatus(true)
                .build();

        responseObserver.onNext(response);
        responseObserver.onCompleted();
    }

    @Override
    public void updateStatus(UpdateStatusRequest request,
                             StreamObserver<UpdateStatusResponse> responseObserver) {

        try {

            AppointmentEntity entity = repository.findById(request.getAppointmentId()).orElse(null);
            if (entity == null) {
                UpdateStatusResponse resp = UpdateStatusResponse.newBuilder()
                    .setStatus(false)
                    .setMensage("Consulta não encontrada: " + request.getAppointmentId())
                    .build();
                responseObserver.onNext(resp);
                responseObserver.onCompleted();
                return;
            }
    
            entity.setStatus(request.getNewStatus().name());
            AppointmentEntity updated = repository.save(entity);
    
            UpdateStatusResponse res = UpdateStatusResponse.newBuilder()
                    .setStatus(true)
                    .setMensage("Status atualizado com sucesso")
                    .setDados(toProto(updated))
                    .build();
    
            responseObserver.onNext(res);
            responseObserver.onCompleted();

        } catch (Exception ex) {
            UpdateStatusResponse resp = UpdateStatusResponse.newBuilder()
                .setStatus(false)
                .setMensage(ex.getMessage())
                .build();

            responseObserver.onNext(resp);
            responseObserver.onCompleted();
        }
    }

    @Override
    public void deleteAppointment(DeleteAppointmentRequest request,
                             StreamObserver<DeleteAppointmentResponse> responseObserver) {

        try {

            AppointmentEntity entity = repository.findById(request.getAppointmentId()).orElse(null);
            if (entity == null) {
                DeleteAppointmentResponse resp = DeleteAppointmentResponse.newBuilder()
                    .setStatus(false)
                    .setMensage("Consulta não encontrada: " + request.getAppointmentId())
                    .build();
                responseObserver.onNext(resp);
                responseObserver.onCompleted();
                return;
            }
    
            repository.delete(entity);
    
            DeleteAppointmentResponse res = DeleteAppointmentResponse.newBuilder()
                    .setStatus(true)
                    .setMensage("Consulta excluída com sucesso")
                    .build();
    
            responseObserver.onNext(res);
            responseObserver.onCompleted();

        } catch (Exception ex) {
            DeleteAppointmentResponse resp = DeleteAppointmentResponse.newBuilder()
                .setStatus(false)
                .setMensage(ex.getMessage())
                .build();

            responseObserver.onNext(resp);
            responseObserver.onCompleted();
        }
    }
}