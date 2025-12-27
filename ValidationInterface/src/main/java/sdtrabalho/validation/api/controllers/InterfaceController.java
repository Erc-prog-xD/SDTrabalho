package sdtrabalho.validation.api.controllers;

import sdtrabalho.validation.interfaces.ValidationService;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.rmi.registry.LocateRegistry;
import java.rmi.registry.Registry;

@RestController
@RequestMapping("/api/validation")
public class InterfaceController {

    @PostMapping
    public ResponseEntity create() {

        try {
            String host = "validation"; // ou nome do serviço no docker-compose
            int port = 1099;

            // Conecta ao RMI Registry
            Registry registry = LocateRegistry.getRegistry(host, port);

            // Lookup do serviço pelo nome definido no RmiServiceExporter
            ValidationService service =
                    (ValidationService) registry.lookup("ValidationService");

            // Chamada remota (RPC/RMI)
            Boolean response =
                    service.VerifyPaymentConfirmation(1,2);

            return ResponseEntity.ok(response);
        } catch (Exception e) {
            e.printStackTrace();
        }

        return new ResponseEntity<>(HttpStatus.BAD_REQUEST);
    }
}