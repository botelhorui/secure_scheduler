package pt.ulisboa.tecnico.sirs.ss;

import java.io.Serializable;
import java.time.LocalDateTime;
import java.util.Date;

/**
 * Created by Engineer on 19-11-2015.
 */
public class Event implements Serializable {
    private LocalDateTime dateTime;
    private String description;

    public Event(LocalDateTime dateTime, String description) {
        this.dateTime = dateTime;
        this.description = description;
    }

    public LocalDateTime getDateTime() {
        return dateTime;
    }

    public void setDateTime(LocalDateTime dateTime) {
        this.dateTime = dateTime;
    }

    public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }


}
