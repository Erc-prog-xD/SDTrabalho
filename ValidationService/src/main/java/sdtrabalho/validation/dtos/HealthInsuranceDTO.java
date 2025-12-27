package sdtrabalho.validation.dtos;

import java.io.Serializable;

public record HealthInsuranceDTO (Integer appointmentId,
    Integer patientId,
    String insuranceName,
    String procedure) implements Serializable { }