package cloud.nativ.flamewars;

import org.eclipse.microprofile.rest.client.inject.RegisterRestClient;

import javax.json.JsonObject;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.QueryParam;
import javax.ws.rs.core.MediaType;

@RegisterRestClient
@Path("/data/2.5/weather")
public interface OpenWeatherMap {
    @GET
    @Consumes(MediaType.APPLICATION_JSON)
    JsonObject getWeather(@QueryParam("q") String city, @QueryParam("APPID") String appid);
}
