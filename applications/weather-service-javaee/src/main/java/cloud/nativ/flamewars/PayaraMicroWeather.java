package cloud.nativ.flamewars;

import javax.json.bind.annotation.JsonbTransient;
import javax.persistence.*;
import java.time.LocalDateTime;

@Entity
@Table(name = "current_weather")
@NamedQuery(name = "findCurrentWeatherByCity", query = "SELECT w FROM PayaraMicroWeather w WHERE w.city = :city AND w.nextUpdate > :now")
public class PayaraMicroWeather {

    PayaraMicroWeather() {
    }

    public PayaraMicroWeather(final String city, final String weather) {
        this.city = city;
        this.weather = weather;
    }

    @Id
    @Column(name = "city", unique = true, nullable = false)
    private String city;

    @Column(name = "weather", nullable = false)
    private String weather;

    @Column(name = "next_update", columnDefinition = "TIMESTAMP")
    @JsonbTransient
    private LocalDateTime nextUpdate;

    public String getCity() {
        return city;
    }

    public String getWeather() {
        return weather;
    }

    public LocalDateTime getNextUpdate() {
        return nextUpdate;
    }

    public PayaraMicroWeather touch(int offset) {
        this.nextUpdate = LocalDateTime.now().plusMinutes(offset);
        return this;
    }

    public void setCity(String city) {
        this.city = city;
    }

    public void setWeather(String weather) {
        this.weather = weather;
    }
}
