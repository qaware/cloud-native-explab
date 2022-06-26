package cloud.nativ.flamewars;

import org.eclipse.microprofile.metrics.annotation.Timed;

import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import javax.validation.constraints.NotBlank;
import javax.ws.rs.GET;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;
import javax.ws.rs.QueryParam;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;

@Path("/weather")
@ApplicationScoped
public class PayaraMicroWeatherResource {

    @Inject
    private PayaraMicroWeatherService service;

    @GET
    @Produces(MediaType.APPLICATION_JSON)
    @Timed(name = "getWeather")
    public Response getWeather(@QueryParam("city") @NotBlank String city) {
        return Response.ok(service.getWeatherForCity(city)).build();
    }
}
