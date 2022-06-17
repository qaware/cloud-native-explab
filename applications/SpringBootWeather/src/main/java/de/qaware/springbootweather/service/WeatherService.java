package de.qaware.springbootweather.service;

import de.qaware.springbootweather.model.Weather;
import de.qaware.springbootweather.provider.WeatherProvider;
import de.qaware.springbootweather.repository.WeatherRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.BeansException;
import org.springframework.beans.factory.NoSuchBeanDefinitionException;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationContext;
import org.springframework.context.ApplicationContextAware;
import org.springframework.stereotype.Component;

import java.sql.Date;
import java.time.Instant;
import java.util.Iterator;

@Component
public class WeatherService implements ApplicationContextAware {

    Logger logger = LoggerFactory.getLogger(WeatherService.class);
    private ApplicationContext applicationContext;

    @Autowired
    private WeatherRepository weatherRepository;

    public String findWeatherByCity(String city) {
        try {
            // First look if the provider gives information to the cities weather
            WeatherProvider provider = applicationContext.getBean(city, WeatherProvider.class);
            Weather weather = new Weather();
            weather.setCity(city);
            weather.setWeather(provider.getWeather());
            weather.setDate(Date.from(Instant.now()));
            logger.info(String.format("Weather for %s successfully retrieved from the provider!", city));
            return weather.toString();
        } catch(NoSuchBeanDefinitionException e) {
            // In case the provider does not give information, the database is used
            Iterable<Weather> weatherForCity = weatherRepository.findWeatherByCity(city);
            if (weatherForCity.iterator().hasNext()) {
                logger.info(String.format("Weather for %s could be retrieved from the database!", city));
                return findMostRecentData(weatherForCity).toString();
            } else {
                // If the database has no information either, the weather is declared unknown.
                return String.format("There is no weather information for %s at the moment", city);
            }
        }
    }

    private Weather findMostRecentData(Iterable<Weather> data) {
        Iterator<Weather> iterator = data.iterator();
        Weather mostRecentWeather = iterator.next();

        while (iterator.hasNext()) {
            Weather nextWeather = iterator.next();
            if (nextWeather.getDate().after(mostRecentWeather.getDate())) {
                mostRecentWeather = nextWeather;
            }
        }

        return mostRecentWeather;
    }

    public String addWeather(String city, String weather) {
        Weather weatherData = new Weather();
        weatherData.setCity(city);
        weatherData.setWeather(weather);
        weatherData.setDate(Date.from(Instant.now()));
        weatherData = weatherRepository.save(weatherData);
        logger.info(String.format("Added weather data with id %s successfully", weatherData.getId()));
        return weatherData.getId().toString();
    }

    public String deleteWeather(Integer id) {
        weatherRepository.deleteWeatherById(id);
        logger.info(String.format("Deleted weather with id %s", id));
        return "Deleted successfully";
    }

    public Weather getWeather(Integer id) {
        logger.info(String.format("Retrieved weather with id %s", id));
        return weatherRepository.findWeatherById(id);
    }

    public Iterable<Weather> listWeather() {
        Iterable<Weather> weatherData = weatherRepository.findAll();
        logger.info("Retrieved all weather data from the database.");
        return weatherData;
    }

    @Override
    public void setApplicationContext(ApplicationContext applicationContext) throws BeansException {
        this.applicationContext = applicationContext;
    }
}
