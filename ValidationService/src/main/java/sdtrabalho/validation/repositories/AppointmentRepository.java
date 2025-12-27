package sdtrabalho.validation.repositories;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

import sdtrabalho.validation.entities.AppointmentEntity;

import java.util.Optional;

@Repository
public interface AppointmentRepository extends JpaRepository<AppointmentEntity, Integer> {
    Optional<AppointmentEntity> findByIdAndPatientId(Integer id, Integer patientId);
}