package pt.ulisboa.tecnico.sirs.ss;

import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.NoSuchPaddingException;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;
import java.io.*;
import java.nio.file.Path;
import java.security.InvalidKeyException;
import java.security.Key;
import java.security.NoSuchAlgorithmException;
import java.security.spec.InvalidParameterSpecException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

/**
 * Created by Engineer on 19-11-2015.
 */
public class Calendar implements Serializable {
    public static final int KEY_LENGHT = 128;
    public static final String CIPHER_MODE = "AES/CBC/PKCS5Padding";
    public static final String KEYGEN_MODE = "AES";
    public transient Path calendarFilePath;
    public transient Path calendarKeyFilePath;
    public transient Path calendarIVFilePath;
    private String containerName;
    private String calendarName;
    private transient List<Event> events;
    private byte[] encodedKey;
    private byte[] encodedIV;

    public Calendar(String name, String containerName) throws NoSuchAlgorithmException, NoSuchPaddingException, InvalidKeyException, InvalidParameterSpecException {
        this.calendarName = name;
        this.events = new ArrayList<>();
        this.containerName = containerName;
        calendarFilePath = getEncryptedCalendarPath(calendarName, containerName);
        calendarFilePath.toFile().getParentFile().mkdirs();
        calendarKeyFilePath = getEncryptionKeyPath(calendarName, containerName);
        calendarKeyFilePath.toFile().getParentFile().mkdirs();
        calendarIVFilePath = getIVPath(calendarName, containerName);
        calendarIVFilePath.toFile().getParentFile().mkdirs();
        //generate key
        KeyGenerator keyGen = KeyGenerator.getInstance(KEYGEN_MODE);
        keyGen.init(KEY_LENGHT);
        Key key = keyGen.generateKey();
        encodedKey = key.getEncoded();
        //generate IV
        Cipher cipher = Cipher.getInstance(CIPHER_MODE);
        cipher.init(Cipher.ENCRYPT_MODE, key);
        IvParameterSpec ivSpec = cipher.getParameters().getParameterSpec(IvParameterSpec.class);
        encodedIV = ivSpec.getIV();
    }

    public static Path getEncryptedCalendarPath(String calendarName, String containerName) {
        return Util.getStoragePath().resolve(containerName).resolve(calendarName + ".sc");
    }

    public static Path getEncryptionKeyPath(String calendarName, String containerName) {
        return Util.getStoragePath().resolve(containerName).resolve(calendarName + ".key");
    }

    public static Path getIVPath(String calendarName, String containerName) {
        return Util.getStoragePath().resolve(containerName).resolve(calendarName + ".iv");
    }

    @Override
    public String toString() {
        return "Calendar{" +
                "containerName='" + containerName + '\'' +
                ", calendarName='" + calendarName + '\'' +
                ", events=" + events +
                ", encodedKey=" + Arrays.toString(encodedKey) +
                ", calendarFilePath=" + calendarFilePath +
                ", encodedIV=" + Arrays.toString(encodedIV) +
                ", calendarKeyFilePath=" + calendarKeyFilePath +
                ", calendarIVFilePath=" + calendarIVFilePath +
                '}';
    }

    private void setEncodedKey(byte[] encodedKey) {
        this.encodedKey = encodedKey;
    }

    public String getCalendarName() {
        return calendarName;
    }

    public void setCalendarName(String calendarName) {
        this.calendarName = calendarName;
    }

    public void addEvent(Event e) {
        events.add(e);
    }

    public List<Event> getEvents() {
        return new ArrayList<>(events);
    }

    public void removeEvent(Event e) {
        events.remove(e);
    }

    private void writeObject(ObjectOutputStream oos) throws IOException {
        oos.defaultWriteObject();

        saveEncryptedCalendar();
    }

    private void readObject(ObjectInputStream ois) throws IOException, ClassNotFoundException {
        ois.defaultReadObject();
        calendarFilePath = getEncryptedCalendarPath(calendarName, containerName);
        calendarKeyFilePath = getEncryptionKeyPath(calendarName, containerName);
        calendarIVFilePath = getIVPath(calendarName, containerName);
        ///read event list from disck
        readCalendar();
    }

    public void saveEncryptedCalendar() throws IOException {
        //create calendar storage folder if are not already created, so we can create the two files
        //write encodedKey
        FileOutputStream fos = null;
        fos = new FileOutputStream(calendarKeyFilePath.toFile());
        fos.write(encodedKey);
        fos.close();
        //write encodedIV
        fos = new FileOutputStream(calendarIVFilePath.toFile());
        fos.write(encodedIV);
        fos.close();
        //serialize events to memory
        ByteArrayOutputStream bos = new ByteArrayOutputStream();
        ObjectOutputStream oos = new ObjectOutputStream(bos);
        oos.writeObject(events);
        oos.close();
        byte[] encodedEvents = bos.toByteArray();
        //encrypt events
        Key key = new SecretKeySpec(encodedKey, KEYGEN_MODE);
        try {
            Cipher cipher = Cipher.getInstance(CIPHER_MODE);
            IvParameterSpec ivSpec = new IvParameterSpec(encodedIV);
            cipher.init(Cipher.ENCRYPT_MODE, key, ivSpec);
            byte[] encryptedEvents = cipher.doFinal(encodedEvents);
            fos = new FileOutputStream(calendarFilePath.toFile());
            fos.write(encryptedEvents);
        } catch (Exception e) {
            throw new RuntimeException("Failed to encrypt calendar", e.getCause());
        } finally {
            fos.close();
        }

    }

    public void readCalendar() throws IOException {
        //read encodedKey
        FileInputStream fis = new FileInputStream(calendarKeyFilePath.toFile());
        byte[] encodedKey = new byte[fis.available()];
        fis.read(encodedKey);
        fis.close();
        //read encodedIV
        fis = new FileInputStream(calendarIVFilePath.toFile());
        byte[] encodedIV = new byte[fis.available()];
        fis.read(encodedIV);
        fis.close();
        //read encryptedEvents
        fis = new FileInputStream(calendarFilePath.toFile());
        byte[] encryptedEvents = new byte[fis.available()];
        fis.read(encryptedEvents);
        fis.close();
        //decrypt encryptedCalendar and get Object serialization
        Key key = new SecretKeySpec(encodedKey, KEYGEN_MODE);
        ByteArrayInputStream bis = null;
        ObjectInputStream ois = null;
        try {
            Cipher cipher = Cipher.getInstance(CIPHER_MODE);
            IvParameterSpec ivSpec = new IvParameterSpec(encodedIV);
            cipher.init(Cipher.DECRYPT_MODE, key, ivSpec);
            byte[] encodedEvents = cipher.doFinal(encryptedEvents);
            //get object instance from serialization
            bis = new ByteArrayInputStream(encodedEvents);
            ois = new ObjectInputStream(bis);
            events = (List<Event>) ois.readObject();
        } catch (Exception e) {
            throw new RuntimeException("Failed to decrypt calendar", e.getCause());
        } finally {
            if (bis != null) bis.close();
            if (ois != null) ois.close();
        }
    }
}
