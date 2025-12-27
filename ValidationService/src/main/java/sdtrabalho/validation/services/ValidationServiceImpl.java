package sdtrabalho.validation.services;

import org.springframework.stereotype.Service;
import java.rmi.RemoteException;
import java.rmi.server.UnicastRemoteObject;

import sdtrabalho.validation.interfaces.ValidationService;

@Service
public class ValidationServiceImpl extends UnicastRemoteObject implements ValidationService {
    public ValidationServiceImpl() throws RemoteException { super(); }

    @Override
    public Boolean VerifyPatientsEligibilityHealthInsuranceProvider(Integer patientId, Integer appointmentId) throws RemoteException {
        return true;
    }

    @Override
    public Boolean VerifyPaymentConfirmation(Integer patientId, Integer appointmentId) throws RemoteException {
        return true;
    }
}
