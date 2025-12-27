package sdtrabalho.validation.services;

import org.springframework.stereotype.Service;
import java.rmi.RemoteException;
import java.rmi.server.UnicastRemoteObject;
import java.util.Set;

import sdtrabalho.validation.dtos.HealthInsuranceDTO;
import sdtrabalho.validation.dtos.PrivatePaymentDTO;
import sdtrabalho.validation.dtos.ValidationResponseDTO;
import sdtrabalho.validation.interfaces.ValidationService;

@Service
public class ValidationServiceImpl extends UnicastRemoteObject implements ValidationService {
    
    private static final Set<String> ACCEPTED_INSURANCES =
        Set.of("UNIMED", "AMIL", "SULAMERICA");

    private static final Set<String> ACCEPTED_PAYMENT_METHODS =
        Set.of("PIX", "CREDITO", "DEBITO");

    public ValidationServiceImpl() throws RemoteException {
        super();
    }

    @Override
    public ValidationResponseDTO validateHealthInsurance(HealthInsuranceDTO healthInsuranceDTO) throws RemoteException {

        if (healthInsuranceDTO.patientId() == null || healthInsuranceDTO.patientId() <= 0)
            return new ValidationResponseDTO(false, "Paciente inválido");

        if (!ACCEPTED_INSURANCES.contains(healthInsuranceDTO.insuranceName().toUpperCase()))
            return new ValidationResponseDTO(false, "Convênio não aceito");

        if (healthInsuranceDTO.procedure() == null || healthInsuranceDTO.procedure().isBlank())
            return new ValidationResponseDTO(false, "Procedimento inválido");

        return new ValidationResponseDTO(true, "Convênio autorizado");
    }

    @Override
    public ValidationResponseDTO validatePrivatePayment(PrivatePaymentDTO privatePaymentDTO) throws RemoteException {

        if (privatePaymentDTO.patientId() == null || privatePaymentDTO.patientId() <= 0)
            return new ValidationResponseDTO(false, "Paciente inválido");

        if (privatePaymentDTO.amount() == null || privatePaymentDTO.amount() <= 0)
            return new ValidationResponseDTO(false, "Valor inválido");

        if (!ACCEPTED_PAYMENT_METHODS.contains(privatePaymentDTO.paymentMethod().toUpperCase()))
            return new ValidationResponseDTO(false, "Forma de pagamento não aceita");

        return new ValidationResponseDTO(true, "Pagamento confirmado");
    }
}
