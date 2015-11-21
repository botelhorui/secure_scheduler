package pt.ulisboa.tecnico.sirs.ss;

import java.io.*;
import java.net.URL;
import java.util.ArrayList;
import java.util.List;

/**
 * Created by Engineer on 19-11-2015.
 */
public class Calendar implements Serializable{
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
        String calendarFileName = String.format("%s.sc",name);
        String absoluteFilePath = Util.getStoragePath() + calendarFileName;
        File calendarFile = new File(absoluteFilePath);
        calendarFile.getParentFile().mkdirs();
        System.out.println(String.format("file path :%s",absoluteFilePath));
        FileOutputStream fos = new FileOutputStream(absoluteFilePath);
        ObjectOutputStream oos = new ObjectOutputStream(fos);
        oos.writeObject(this);
        oos.close();
    }
}
