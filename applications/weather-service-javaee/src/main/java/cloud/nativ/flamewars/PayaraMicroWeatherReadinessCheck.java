package cloud.nativ.flamewars;

import org.eclipse.microprofile.health.HealthCheck;
import org.eclipse.microprofile.health.HealthCheckResponse;
import org.eclipse.microprofile.health.Readiness;

import javax.enterprise.context.ApplicationScoped;

@Readiness
@ApplicationScoped
public class PayaraMicroWeatherReadinessCheck implements HealthCheck {

    @Override
    public HealthCheckResponse call() {
        return HealthCheckResponse.named(PayaraMicroWeatherReadinessCheck.class.getSimpleName()).withData("ready", true).up().build();
    }
}
