//package sdtrabalho.validation.configuration;
//
//import org.springframework.context.annotation.Bean;
//import org.springframework.context.annotation.Configuration;
//import org.springframework.remoting.rmi.RmiServiceExporter;
//
//import sdtrabalho.validation.interfaces.ValidationService;
//import sdtrabalho.validation.services.ValidationServiceImpl;
//
//@Configuration
//public class AppConfig {
//
//    @Bean
//    public RmiServiceExporter rmiServiceExporter(ValidationService validationService) throws Exception {
//        RmiServiceExporter exporter = new RmiServiceExporter();
//        exporter.setServiceName("ValidationService");
//        exporter.setServiceInterface(ValidationService.class);
//        exporter.setService(validationService);
//        exporter.setRegistryPort(1099);
//        return exporter;
//    }
//}