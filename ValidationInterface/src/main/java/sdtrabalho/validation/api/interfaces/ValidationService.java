package sdtrabalho.validation.interfaces;

import java.rmi.Remote;
import java.rmi.RemoteException;

import sdtrabalho.validation.dtos.HealthInsuranceDTO;
import sdtrabalho.validation.dtos.PrivatePaymentDTO;
import sdtrabalho.validation.dtos.ValidationResponseDTO;

public interface ValidationService extends Remote {
    ValidationResponseDTO validateHealthInsurance(
        HealthInsuranceDTO healthInsuranceDTO
    ) throws RemoteException;

    ValidationResponseDTO validatePrivatePayment(
        PrivatePaymentDTO privatePaymentDTO
    ) throws RemoteException;
}