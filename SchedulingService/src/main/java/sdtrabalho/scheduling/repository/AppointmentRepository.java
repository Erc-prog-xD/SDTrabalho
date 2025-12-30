package sdtrabalho.scheduling.repository;


import org.springframework.data.jpa.repository.Modifying;
import org.springframework.transaction.annotation.Transactional;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;
import sdtrabalho.scheduling.entity.AppointmentEntity;

import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import java.time.LocalDateTime;
import java.util.List;

@Repository
public interface AppointmentRepository extends JpaRepository<AppointmentEntity, Integer> {

    List<AppointmentEntity> findByPatientId(Integer patientId);

    List<AppointmentEntity> findByDoctorId(Integer doctorId);

    @Query(value = """
        SELECT *
        FROM clinica_db..appointments a
        WHERE :start BETWEEN a.datetime AND DATEADD(MINUTE, 29, a.datetime)
          AND a.status <> :status
          AND (a.doctor_id = :doctorId OR a.patient_id = :patientId)
        """, nativeQuery = true)
    List<AppointmentEntity> findAppointmentConflicts(
        @Param("start") LocalDateTime start,
        @Param("status") String status,
        @Param("doctorId") Integer doctorId,
        @Param("patientId") Integer patientId
    );

    @Query(value = """
        SELECT CASE WHEN EXISTS (
            SELECT 1
            FROM clinica_db..users u
            WHERE u.Role = 0
            AND u.Id = :patientId
            AND u.DeletionDate IS NULL
        ) THEN 1 ELSE 0 END
        """, nativeQuery = true)
        
    int verifyExistsPacient(
        @Param("patientId") Integer patientId
    );

    @Query(value = """
        SELECT CASE WHEN EXISTS (
            SELECT 1
            FROM clinica_db..users u
            WHERE u.Role = 1
            AND u.Id = :doctorId
            AND u.DeletionDate IS NULL
        ) THEN 1 ELSE 0 END
        """, nativeQuery = true)
    int verifyExistsMedic(
        @Param("doctorId") Integer doctorId
    );

    @Modifying
    @Transactional
    @Query(value = """
        INSERT INTO clinica_db..Notifications
            (DoctorId, PatientId, AppointmentId, Status, Message, CreatedAt, Published, DeletionDate)
        VALUES
            (:doctorId, :patientId, :appointmentId, :status, :message, GETDATE(), 0, NULL)
        """, nativeQuery = true)
    void insertNotification(
        @Param("doctorId") Integer doctorId,
        @Param("patientId") Integer patientId,
        @Param("appointmentId") Integer appointmentId,
        @Param("status") String status,
        @Param("message") String message
    );
}