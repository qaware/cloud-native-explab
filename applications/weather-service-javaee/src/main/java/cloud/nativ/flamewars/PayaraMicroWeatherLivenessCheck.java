package cloud.nativ.flamewars;

import org.eclipse.microprofile.health.HealthCheck;
import org.eclipse.microprofile.health.HealthCheckResponse;
import org.eclipse.microprofile.health.Liveness;

import javax.enterprise.context.ApplicationScoped;

@Liveness
@ApplicationScoped
public class PayaraMicroWeatherLivenessCheck implements HealthCheck {
    @Override
    public HealthCheckResponse call() {
        return HealthCheckResponse.named(PayaraMicroWeatherLivenessCheck.class.getSimpleName()).withData("live", true).up().build();
    }
}
