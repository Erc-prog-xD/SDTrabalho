package sdtrabalho.validation.dtos;

import java.io.Serializable;

public record ValidationResponseDTO (Boolean approved, String message) implements Serializable { }