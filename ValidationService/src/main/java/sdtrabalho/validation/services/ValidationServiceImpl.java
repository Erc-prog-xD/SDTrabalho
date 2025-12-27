package sdtrabalho.validation.services;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.rmi.RemoteException;
import java.rmi.server.UnicastRemoteObject;
import java.util.Optional;
import java.util.Set;

import sdtrabalho.validation.dtos.HealthInsuranceDTO;
import sdtrabalho.validation.dtos.PrivatePaymentDTO;
import sdtrabalho.validation.dtos.ValidationResponseDTO;
import sdtrabalho.validation.interfaces.ValidationService;
import sdtrabalho.validation.repositories.AppointmentRepository;
import sdtrabalho.validation.entities.AppointmentEntity;

@Service
public class ValidationServiceImpl extends UnicastRemoteObject implements ValidationService {
    
    @Autowired
    private AppointmentRepository repository;

    private static final Set<String> ACCEPTED_INSURANCES =
        Set.of("UNIMED", "AMIL", "SULAMERICA");

    private static final Set<String> ACCEPTED_PAYMENT_METHODS =
        Set.of("PIX", "CREDITO", "DEBITO");

    public ValidationServiceImpl() throws RemoteException {
        super();
    }

    @Override
    public ValidationResponseDTO validateHealthInsurance(HealthInsuranceDTO healthInsuranceDTO) throws RemoteException {

        if (healthInsuranceDTO.appointmentId() == null || healthInsuranceDTO.appointmentId() <= 0)
            return new ValidationResponseDTO(false, "Agendamento inválido");

        if (healthInsuranceDTO.patientId() == null || healthInsuranceDTO.patientId() <= 0)
            return new ValidationResponseDTO(false, "Paciente inválido");

        Optional<AppointmentEntity> appointment = repository.findByIdAndPatientId(healthInsuranceDTO.appointmentId(), healthInsuranceDTO.patientId());

        if (appointment.isEmpty())
            return new ValidationResponseDTO(false, "Agendamento não encontrado");

        if (!ACCEPTED_INSURANCES.contains(healthInsuranceDTO.insuranceName().toUpperCase())) {
            appointment.get().setStatus("STATUS_CANCELLED");
            repository.save(appointment.get());
            return new ValidationResponseDTO(false, "Convênio não aceito");
        }

        if (healthInsuranceDTO.procedure() == null || healthInsuranceDTO.procedure().isBlank()) {
            appointment.get().setStatus("STATUS_CANCELLED");
            repository.save(appointment.get());
            return new ValidationResponseDTO(false, "Procedimento inválido");
        }

        appointment.get().setStatus("STATUS_CONFIRMED");
        repository.save(appointment.get());

        return new ValidationResponseDTO(true, "Convênio autorizado");
    }

    @Override
    public ValidationResponseDTO validatePrivatePayment(PrivatePaymentDTO privatePaymentDTO) throws RemoteException {

        if (privatePaymentDTO.appointmentId() == null || privatePaymentDTO.appointmentId() <= 0)
            return new ValidationResponseDTO(false, "Agendamento inválido");

        if (privatePaymentDTO.patientId() == null || privatePaymentDTO.patientId() <= 0)
            return new ValidationResponseDTO(false, "Paciente inválido");

        Optional<AppointmentEntity> appointment = repository.findByIdAndPatientId(privatePaymentDTO.appointmentId(), privatePaymentDTO.patientId());

        if (appointment.isEmpty())
            return new ValidationResponseDTO(false, "Agendamento não encontrado");

        if (privatePaymentDTO.amount() == null || privatePaymentDTO.amount() <= 0)
            return new ValidationResponseDTO(false, "Valor inválido");

        if (!ACCEPTED_PAYMENT_METHODS.contains(privatePaymentDTO.paymentMethod().toUpperCase())) {
            appointment.get().setStatus("STATUS_CANCELLED");
            repository.save(appointment.get());
            return new ValidationResponseDTO(false, "Forma de pagamento não aceita");
        }

        appointment.get().setStatus("STATUS_CONFIRMED");
        repository.save(appointment.get());

        return new ValidationResponseDTO(true, "Pagamento confirmado");
    }
}
