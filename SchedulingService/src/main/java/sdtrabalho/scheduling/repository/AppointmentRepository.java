package sdtrabalho.scheduling.repository;


import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;
import sdtrabalho.scheduling.entity.AppointmentEntity;

import java.util.List;

@Repository
public interface AppointmentRepository extends JpaRepository<AppointmentEntity, Long> {

    List<AppointmentEntity> findByPatientId(String patientId);

    List<AppointmentEntity> findByDoctorId(String doctorId);
}