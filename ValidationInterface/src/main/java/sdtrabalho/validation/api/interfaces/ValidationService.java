package sdtrabalho.validation.interfaces;

import java.rmi.Remote;
import java.rmi.RemoteException;

public interface ValidationService extends Remote {
    public Boolean VerifyPatientsEligibilityHealthInsuranceProvider(Integer patientId, Integer appointmentId) throws RemoteException;
    public Boolean VerifyPaymentConfirmation(Integer patientId, Integer appointmentId) throws RemoteException;
}