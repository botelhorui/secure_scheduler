package pt.ulisboa.tecnico.sirs.ss;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.ObjectInputStream;
import java.security.NoSuchAlgorithmException;
import java.time.LocalDateTime;

public class Main {

    public static void main(String[] args) throws Exception {
        Event e1 = new Event(LocalDateTime.of(2015, 11, 20, 14, 0), "Almoço");
        Event e2 = new Event(LocalDateTime.of(2015, 11, 20, 15, 0), "Soneca");
        Calendar ruiCal = new Calendar("Rui");
        ruiCal.addEvent(e1);
        ruiCal.addEvent(e2);
        ruiCal.addEvent(new Event(LocalDateTime.now(), "Coding"));
        ruiCal.addEvent(new Event(LocalDateTime.now().plusHours(4), "Lunch"));
        ruiCal.saveCalendar();

        Calendar ruiCalClone = Calendar.readCalendar("Rui");
        System.out.println("Calendar 'Rui'");
        for (Event e : ruiCalClone.getEvents()) {
            System.out.println(" - " + e.getDescription());
        }

        Calendar c = new Calendar("Laura");
        c.addEvent(new Event(LocalDateTime.now(), "Coding"));
        c.addEvent(new Event(LocalDateTime.now().plusHours(4), "Lunch"));
        c.saveCalendar();

        Calendar c2 = Calendar.readCalendar("Laura");
        System.out.println("Calendar 'Laura'");
        for (Event e : c2.getEvents()) {
            System.out.println(" - " + e.getDescription());
        }
    }
}

