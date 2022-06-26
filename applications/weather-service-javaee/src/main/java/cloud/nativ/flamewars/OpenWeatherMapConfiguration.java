package cloud.nativ.flamewars;

import org.eclipse.microprofile.config.inject.ConfigProperty;

import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;

@ApplicationScoped
public class OpenWeatherMapConfiguration {
    @Inject
    @ConfigProperty(name = "weather.appid", defaultValue = "5b3f51e527ba4ee2ba87940ce9705cb5")
    private String weatherAppId;

    @Inject
    @ConfigProperty(name = "weather.uri", defaultValue = "https://api.openweathermap.org")
    private String weatherUri;

    public String getWeatherAppId() {
        return weatherAppId;
    }

    public String getWeatherUri() {
        return weatherUri;
    }
}
