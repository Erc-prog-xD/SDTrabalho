package sdtrabalho.validation;

import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;

import java.rmi.registry.LocateRegistry;
import java.rmi.registry.Registry;

import sdtrabalho.validation.interfaces.ValidationService;
import sdtrabalho.validation.services.ValidationServiceImpl;

@SpringBootApplication
public class ValidationApplication {

    public static void main(String[] args) {
        SpringApplication.run(ValidationApplication.class, args);
    }

    @Bean
    CommandLineRunner rmiStarter(ValidationService validationService) {
        return args -> {
            Registry registry = LocateRegistry.createRegistry(1099);
            registry.rebind("ValidationService", validationService);
            System.out.println("✅ RMI Registry 1099 OK, serviço 'ValidationService' registrado");
        };
    }
}
