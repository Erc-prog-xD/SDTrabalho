package sdtrabalho.scheduling.entity;

import jakarta.persistence.*;
import lombok.Data;

import java.time.LocalDateTime;

@Entity
@Table(name = "[Appointments]")
@Data
public class AppointmentEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "Id")
    private Integer id;

    @Column(name = "PatientId", nullable = false)
    private Integer patientId;

    @Column(name = "DoctorId", nullable = false)
    private Integer doctorId;

    @Column(name = "Specialty", nullable = false)
    private String specialty;

    @Column(name = "Datetime", nullable = false)
    private LocalDateTime datetime;

    @Column(name = "Status", nullable = false)
    private String status;
}
