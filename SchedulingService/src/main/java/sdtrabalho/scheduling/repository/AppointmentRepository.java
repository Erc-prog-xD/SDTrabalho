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
        FROM clinica_db..Appointments a
        WHERE :start BETWEEN a.Datetime AND DATEADD(MINUTE, 29, a.Datetime)
          AND a.Status <> :status
          AND (a.DoctorId = :doctorId OR a.PatientId = :patientId)
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
}