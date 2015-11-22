package pt.ulisboa.tecnico.sirs.ss;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;

/**
 * Created by Rui on 21-11-2015.
 */
public class User implements Serializable {
    private List<Calendar> calendars = new ArrayList<>();
    private String name;

    public User(String name) {
        this.name = name;
    }

    public List<Calendar> getCalendars() {
        return calendars;
    }

    public void setCalendars(List<Calendar> calendars) {
        this.calendars = calendars;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    @Override
    public String toString() {
        return "User{" +
                "calendars=" + calendars +
                ", name='" + name + '\'' +
                '}';
    }
}
