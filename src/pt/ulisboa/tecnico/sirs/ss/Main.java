package pt.ulisboa.tecnico.sirs.ss;

import java.io.FileInputStream;
import java.io.IOException;
import java.io.ObjectInputStream;
import java.time.LocalDateTime;

public class Main {

    public static void main(String[] args) throws IOException, ClassNotFoundException {
	    Event e1 = new Event(LocalDateTime.of(2015,11,20,14,0),"Almoço");
        Event e2 = new Event(LocalDateTime.of(2015,11,20,15,0),"Soneca");
        Calendar c = new Calendar("Rui");
        c.addEvent(e1);
        c.addEvent(e2);
        c.saveCalendar();
        FileInputStream fis = new FileInputStream(Main.class.);
        ObjectInputStream ois = new ObjectInputStream(fis);
        Calendar c2 = (Calendar)ois.readObject();
        for (Event e : c2.getEvents()
                ) {
            System.out.println(e.getDescription());
        }
    }
}
