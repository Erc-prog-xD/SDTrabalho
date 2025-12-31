package sdtrabalho.validation.api.controllers;

import sdtrabalho.validation.dtos.HealthInsuranceDTO;
import sdtrabalho.validation.dtos.PrivatePaymentDTO;
import sdtrabalho.validation.dtos.ValidationResponseDTO;
import sdtrabalho.validation.interfaces.ValidationService;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.security.access.prepost.PreAuthorize;

import java.rmi.RemoteException;
import java.rmi.registry.LocateRegistry;
import java.rmi.registry.Registry;

@RestController
@RequestMapping("/api/validation")
public class InterfaceController {

    private final String HOST = "validation";
    private final Integer PORT = 1099;
    private final String SERVICE_NAME = "ValidationService";

    private ValidationService connection() {
        try {

            Registry registry = LocateRegistry.getRegistry(HOST, PORT);

            return (ValidationService) registry.lookup(SERVICE_NAME);

        } catch (Exception ex) {
            throw new RuntimeException(ex.getMessage());

        }
    }

    @PostMapping("/health-insurance")
    @PreAuthorize("hasAnyRole('Admin','Recepcionista','Medico')")
    public ResponseEntity validateHealthInsurance(@RequestBody HealthInsuranceDTO healthInsuranceDTO) {
        
        try {

            ValidationService service = connection();

            ValidationResponseDTO response = service.validateHealthInsurance(healthInsuranceDTO);

            return ResponseEntity.ok(response);

        } catch (Exception ex) {

            return ResponseEntity.status(503).body(
                new ValidationResponseDTO(false, ex.getMessage())
            );

        }
    }

    @PostMapping("/private-payment")
    @PreAuthorize("hasAnyRole('Paciente')")
    public ResponseEntity validatePrivatePayment(@RequestBody PrivatePaymentDTO privatePaymentDTO) {
        
        try {

            ValidationService service = connection();

            ValidationResponseDTO response = service.validatePrivatePayment(privatePaymentDTO);

            return ResponseEntity.ok(response);

        } catch (Exception ex) {

            return ResponseEntity.status(503).body(
                new ValidationResponseDTO(false, ex.getMessage())
            );

        }
    }
}