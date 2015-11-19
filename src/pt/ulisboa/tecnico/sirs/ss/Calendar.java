package pt.ulisboa.tecnico.sirs.ss;

import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.ObjectOutputStream;
import java.util.ArrayList;
import java.util.List;

/**
 * Created by Engineer on 19-11-2015.
 */
public class Calendar {
    private String name;
    private List<Event> events;

    public Calendar(String name) {
        this.name = name;
        this.events = new ArrayList<>();
    }

    public void addEvent(Event e){
        events.add(e);
    }

    public List<Event> getEvents(){
        return new ArrayList<>(events);
    }

    public void deleteEvent(Event e){
        events.remove(e);
    }

    void saveCalendar() throws IOException {
        FileOutputStream fos = new FileOutputStream(String.format("/Calendars/%s.lol",name));
        ObjectOutputStream oos = new ObjectOutputStream(fos);
        oos.writeObject(this);
        oos.close();
    }
}
