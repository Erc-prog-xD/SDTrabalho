package sdtrabalho.scheduling.entity;

import jakarta.persistence.*;
import lombok.Data;

import java.time.LocalDateTime;

@Entity
@Table(name = "[Notifications]")
@Data
public class NotificationEntity {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "Id")
    private Integer id;

    @Column(name = "AppointmentId")
    private Integer appointmentId;

    @Column(name = "PatientId")
    private Integer patientId;

    @Column(name = "DoctorId")
    private Integer doctorId;

    @Column(name = "Message")
    private String message;

    @Column(name = "CreatedAt")
    private LocalDateTime createdAt;

    @Column(name = "Status")
    private String status;

    @Column(name = "Published")
    private Boolean published;

    @Column(name = "DeletionDate")
    private LocalDateTime deletionDate;
}