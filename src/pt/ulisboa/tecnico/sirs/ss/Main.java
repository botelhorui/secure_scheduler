package pt.ulisboa.tecnico.sirs.ss;

import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;
import java.nio.file.Path;
import java.time.LocalDateTime;

public class Main {

    public static void main(String[] args) throws Exception {
        UserManager um = new UserManager();
        //RUI
        User rui = new User("Rui Botelho");
        um.getUsers().add(rui);
        Calendar ruical = new Calendar("Rui's calendar", "rui");
        ruical.addEvent(new Event(LocalDateTime.now(), "LOL"));
        rui.getCalendars().add(ruical);
        //LAURA
        User laura = new User("Laura Gouveia");
        um.getUsers().add(laura);
        Calendar lauracal = new Calendar("Laura's calendar", "laura");
        lauracal.addEvent(new Event(LocalDateTime.now(), "FCT"));
        laura.getCalendars().add(lauracal);
        //TASNEEM
        User tasneem = new User("Tasneem Akhthar");
        um.getUsers().add(tasneem);
        Calendar tasneemcal = new Calendar("Tasneem's calendar", "tasneem");
        tasneemcal.addEvent(new Event(LocalDateTime.now(), "Sleeping"));
        tasneem.getCalendars().add(tasneemcal);

        System.out.println(um);
    }

    public static void t1() throws Exception {
        Event e1 = new Event(LocalDateTime.of(2015, 11, 20, 14, 0), "Almoço");
        Event e2 = new Event(LocalDateTime.of(2015, 11, 20, 15, 0), "Soneca");
        Calendar ruiCal = new Calendar("Rui", "rui");
        ruiCal.addEvent(e1);
        ruiCal.addEvent(e2);
        ruiCal.addEvent(new Event(LocalDateTime.now(), "Coding"));
        ruiCal.addEvent(new Event(LocalDateTime.now().plusHours(4), "Lunch"));

        Path p = Util.getStoragePath().resolve("ruiCal.cal");
        new ObjectOutputStream(new FileOutputStream(p.toFile())).writeObject(ruiCal);
        Calendar ruiCalClone = (Calendar) new ObjectInputStream(new FileInputStream(p.toFile())).readObject();
        System.out.println("Calendar 'Rui'");
        System.out.println(ruiCalClone);
        for (Event e : ruiCalClone.getEvents()) {
            System.out.println(" - " + e.getDescription());
        }

        Calendar c = new Calendar("Laura", "laura");
        c.addEvent(new Event(LocalDateTime.now(), "Coding"));
        c.addEvent(new Event(LocalDateTime.now().plusHours(4), "Lunch"));

        Path p2 = Util.getStoragePath().resolve("lauraCal.cal");
        new ObjectOutputStream(new FileOutputStream(p2.toFile())).writeObject(ruiCal);
        Calendar c2 = (Calendar) new ObjectInputStream(new FileInputStream(p2.toFile())).readObject();
        System.out.println("Calendar 'Laura'");
        for (Event e : c2.getEvents()) {
            System.out.println(" - " + e.getDescription());
        }
    }
}

