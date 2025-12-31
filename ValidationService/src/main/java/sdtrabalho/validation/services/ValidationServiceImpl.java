package sdtrabalho.validation.services;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.rmi.RemoteException;
import java.rmi.server.UnicastRemoteObject;
import java.util.Optional;
import java.util.Set;
import java.time.LocalDateTime;

import sdtrabalho.validation.dtos.HealthInsuranceDTO;
import sdtrabalho.validation.dtos.PrivatePaymentDTO;
import sdtrabalho.validation.dtos.ValidationResponseDTO;
import sdtrabalho.validation.interfaces.ValidationService;
import sdtrabalho.validation.repositories.AppointmentRepository;
import sdtrabalho.validation.entities.AppointmentEntity;
import sdtrabalho.validation.repositories.NotificationRepository;
import sdtrabalho.validation.entities.NotificationEntity;

@Service
public class ValidationServiceImpl extends UnicastRemoteObject implements ValidationService {
    
    @Autowired
    private AppointmentRepository appointmentRepository;
    @Autowired
    private NotificationRepository notificationRepository;

    private static final Set<String> ACCEPTED_INSURANCES =
        Set.of("UNIMED", "AMIL", "SULAMERICA");

    private static final Set<String> ACCEPTED_PAYMENT_METHODS =
        Set.of("PIX", "CREDITO", "DEBITO");

    public ValidationServiceImpl() throws RemoteException {
        super();
    }

    private NotificationEntity toEntity(            
        Integer appointmentId,
        Integer patientId,
        Integer doctorId,
        String message,
        String status) {
        NotificationEntity e = new NotificationEntity();
        e.setAppointmentId(appointmentId);
        e.setPatientId(patientId);
        e.setDoctorId(doctorId);
        e.setMessage(message);
        e.setCreatedAt(LocalDateTime.now());
        e.setStatus(status);
        e.setPublished(false);
        return e;
    }

    @Override
    public ValidationResponseDTO validateHealthInsurance(HealthInsuranceDTO healthInsuranceDTO) throws RemoteException {

        if (healthInsuranceDTO.appointmentId() == null || healthInsuranceDTO.appointmentId() <= 0)
            return new ValidationResponseDTO(false, "Agendamento inválido");

        if (healthInsuranceDTO.patientId() == null || healthInsuranceDTO.patientId() <= 0)
            return new ValidationResponseDTO(false, "Paciente inválido");

        Optional<AppointmentEntity> appointment = appointmentRepository.findByIdAndPatientId(healthInsuranceDTO.appointmentId(), healthInsuranceDTO.patientId());

        if (appointment.isEmpty())
            return new ValidationResponseDTO(false, "Agendamento não encontrado");

        if (!ACCEPTED_INSURANCES.contains(healthInsuranceDTO.insuranceName().toUpperCase())) {
            appointment.get().setStatus("STATUS_CANCELLED");
            appointmentRepository.save(appointment.get());
            NotificationEntity notification = toEntity(
                healthInsuranceDTO.appointmentId(),
                healthInsuranceDTO.patientId(),
                appointment.get().getDoctorId(),
                "Convênio não aceito",
                "STATUS_CANCELLED");
            
            notificationRepository.save(notification);
            return new ValidationResponseDTO(false, "Convênio não aceito");
        }

        if (healthInsuranceDTO.procedure() == null || healthInsuranceDTO.procedure().isBlank()) {
            appointment.get().setStatus("STATUS_CANCELLED");
            appointmentRepository.save(appointment.get());
            NotificationEntity notification = toEntity(
                healthInsuranceDTO.appointmentId(),
                healthInsuranceDTO.patientId(),
                appointment.get().getDoctorId(),
                "Procedimento inválido",
                "STATUS_CANCELLED");
            
            notificationRepository.save(notification);
            return new ValidationResponseDTO(false, "Procedimento inválido");
        }

        appointment.get().setStatus("STATUS_CONFIRMED");
        appointmentRepository.save(appointment.get());

        NotificationEntity notification = toEntity(
            healthInsuranceDTO.appointmentId(),
            healthInsuranceDTO.patientId(),
            appointment.get().getDoctorId(),
            "Convênio autorizado com sucesso",
            "STATUS_CONFIRMED");
        
        notificationRepository.save(notification);

        return new ValidationResponseDTO(true, "Convênio autorizado");
    }

    @Override
    public ValidationResponseDTO validatePrivatePayment(PrivatePaymentDTO privatePaymentDTO) throws RemoteException {

        if (privatePaymentDTO.appointmentId() == null || privatePaymentDTO.appointmentId() <= 0)
            return new ValidationResponseDTO(false, "Agendamento inválido");

        if (privatePaymentDTO.patientId() == null || privatePaymentDTO.patientId() <= 0)
            return new ValidationResponseDTO(false, "Paciente inválido");

        Optional<AppointmentEntity> appointment = appointmentRepository.findByIdAndPatientId(privatePaymentDTO.appointmentId(), privatePaymentDTO.patientId());

        if (appointment.isEmpty())
            return new ValidationResponseDTO(false, "Agendamento não encontrado");

        if (privatePaymentDTO.amount() == null || privatePaymentDTO.amount() <= 0)
            return new ValidationResponseDTO(false, "Valor inválido");

        if (!ACCEPTED_PAYMENT_METHODS.contains(privatePaymentDTO.paymentMethod().toUpperCase())) {
            appointment.get().setStatus("STATUS_CANCELLED");
            appointmentRepository.save(appointment.get());
            NotificationEntity notification = toEntity(
                privatePaymentDTO.appointmentId(),
                privatePaymentDTO.patientId(),
                appointment.get().getDoctorId(),
                "Forma de pagamento não aceita",
                "STATUS_CANCELLED");
            notificationRepository.save(notification);
            return new ValidationResponseDTO(false, "Forma de pagamento não aceita");
        }

        appointment.get().setStatus("STATUS_CONFIRMED");
        appointmentRepository.save(appointment.get());

        NotificationEntity notification = toEntity(
            privatePaymentDTO.appointmentId(),
            privatePaymentDTO.patientId(),
            appointment.get().getDoctorId(),
            "Pagamento confirmado com sucesso",
            "STATUS_CONFIRMED");
        notificationRepository.save(notification);

        return new ValidationResponseDTO(true, "Pagamento confirmado");
    }
}
