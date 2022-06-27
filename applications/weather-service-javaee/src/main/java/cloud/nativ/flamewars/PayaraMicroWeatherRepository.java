package cloud.nativ.flamewars;

import javax.ejb.Stateless;
import javax.ejb.TransactionAttribute;
import javax.ejb.TransactionAttributeType;
import javax.persistence.EntityManager;
import javax.persistence.PersistenceContext;
import javax.persistence.TypedQuery;
import javax.transaction.Transactional;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

@Stateless
@TransactionAttribute(TransactionAttributeType.REQUIRED)
@Transactional
public class PayaraMicroWeatherRepository {

    @PersistenceContext
    private EntityManager em;

    public PayaraMicroWeather save(PayaraMicroWeather currentWeather) {
        return em.merge(currentWeather);
    }

    public Optional<PayaraMicroWeather> findCurrentWeatherByCity(String city) {
        TypedQuery<PayaraMicroWeather> query = em.createNamedQuery("findCurrentWeatherByCity", PayaraMicroWeather.class);
        query.setParameter("city", city);
        query.setParameter("now", LocalDateTime.now());

        List<PayaraMicroWeather> results = query.getResultList();
        if (results.isEmpty()) {
            return Optional.empty();
        } else {
            return Optional.of(results.get(0));
        }
    }
}
