package sdtrabalho.scheduling.entity;

import jakarta.persistence.*;
import lombok.Data;

@Entity
@Table(name = "appointments")
@Data
public class AppointmentEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @Column(name = "patient_id", nullable = false)
    private String patientId;

    @Column(name = "doctor_id", nullable = false)
    private String doctorId;

    @Column(name = "specialty", nullable = false)
    private String specialty;

    @Column(name = "datetime_iso", nullable = false)
    private String datetimeIso;

    @Column(name = "status", nullable = false)
    private String status; // vamos guardar o enum como String
}
