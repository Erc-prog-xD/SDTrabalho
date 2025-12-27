package sdtrabalho.validation.dtos;

import java.io.Serializable;

public record PrivatePaymentDTO (Integer appointmentId,
    Integer patientId,
    Double amount,
    String paymentMethod) implements Serializable { }