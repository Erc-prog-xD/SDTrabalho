package sdtrabalho.scheduling.repository;


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
}