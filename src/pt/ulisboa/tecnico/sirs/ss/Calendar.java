package pt.ulisboa.tecnico.sirs.ss;

import javax.crypto.*;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;
import java.io.*;
import java.security.InvalidAlgorithmParameterException;
import java.security.InvalidKeyException;
import java.security.Key;
import java.security.NoSuchAlgorithmException;
import java.security.spec.InvalidParameterSpecException;
import java.util.ArrayList;
import java.util.List;

/**
 * Created by Engineer on 19-11-2015.
 */
public class Calendar implements Serializable {
    public static final int KeyLenght = 128;
    public static final String CIPHER_MODE = "AES/CBC/PKCS5Padding";
    public static final String KEYGEN_MODE = "AES";


    private String calendarName;
    private List<Event> events;


    private transient byte[] encodedKey;
    private transient byte[] encodedIV;

    public final String calendarFilePath;
    public final String calendarKeyFilePath;
    public final String calendarIVFilePath;

    private void setEncodedKey(byte[] encodedKey) {
        this.encodedKey = encodedKey;
    }

    public Calendar(String name) throws NoSuchAlgorithmException, NoSuchPaddingException, InvalidKeyException, InvalidParameterSpecException {
        this.calendarName = name;
        this.events = new ArrayList<>();
        calendarFilePath = Util.getStoragePath() + calendarName + ".sc";
        calendarKeyFilePath = Util.getStoragePath() + calendarName + ".key";
        calendarIVFilePath = Util.getStoragePath() + calendarName + ".iv";
        //generate key
        KeyGenerator keyGen = KeyGenerator.getInstance(KEYGEN_MODE);
        keyGen.init(KeyLenght);
        Key key = keyGen.generateKey();
        encodedKey = key.getEncoded();
        //generate IV
        Cipher cipher = Cipher.getInstance(CIPHER_MODE);
        cipher.init(Cipher.ENCRYPT_MODE, key);
        IvParameterSpec ivSpec = cipher.getParameters().getParameterSpec(IvParameterSpec.class);
        encodedIV = ivSpec.getIV();
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

    void saveCalendar() throws IOException, NoSuchPaddingException, NoSuchAlgorithmException, InvalidKeyException,
            InvalidParameterSpecException, InvalidAlgorithmParameterException, BadPaddingException,
            IllegalBlockSizeException {
        /*
            TODO we should use try catch. declare all variables before.
            put all instructions inside try/catch. In the finally block
            for each resource we do "if(out!=null)out.close; "
         */

        File calendarFile = new File(calendarFilePath);
        //create calendar storage folder if are not already created, so we can create the two files
        calendarFile.getParentFile().mkdirs();
        //write encodedKey
        FileOutputStream fos = new FileOutputStream(calendarKeyFilePath);
        fos.write(encodedKey);
        fos.close();
        //write encodedIV
        fos = new FileOutputStream(calendarIVFilePath);
        fos.write(encodedIV);
        fos.close();
        //serialize object to memory
        ByteArrayOutputStream bos = new ByteArrayOutputStream();
        ObjectOutputStream oos = new ObjectOutputStream(bos);
        oos.writeObject(this);
        oos.close();
        byte[] encodedCalendar = bos.toByteArray();
        //encrypt serializable object
        Key key = new SecretKeySpec(encodedKey, KEYGEN_MODE);
        Cipher cipher = Cipher.getInstance(CIPHER_MODE);
        IvParameterSpec ivSpec = new IvParameterSpec(encodedIV);
        cipher.init(Cipher.ENCRYPT_MODE, key, ivSpec);
        byte[] encryptedCalendar = cipher.doFinal(encodedCalendar);
        fos = new FileOutputStream(calendarFilePath);
        fos.write(encryptedCalendar);
        fos.close();
    }

    public static Calendar readCalendar(String calendarName) throws IOException, ClassNotFoundException, BadPaddingException, IllegalBlockSizeException, InvalidAlgorithmParameterException, InvalidKeyException, NoSuchPaddingException, NoSuchAlgorithmException {
           /*
            TODO we should use try catch. declare all variables before.
            put all instructions inside try/catch. In the finally block
            for each resource we do "if(out!=null)out.close; "
         */
        String calendarFilePath = Util.getStoragePath() + calendarName + ".sc";
        String calendarKeyFilePath = Util.getStoragePath() + calendarName + ".key";
        String calendarIVFilePath = Util.getStoragePath() + calendarName + ".iv";
        //read encodedKey
        FileInputStream fis = new FileInputStream(calendarKeyFilePath);
        byte[] encodedKey = new byte[fis.available()];
        fis.read(encodedKey);
        fis.close();
        //read encodedIV
        fis = new FileInputStream(calendarIVFilePath);
        byte[] encodedIV = new byte[fis.available()];
        fis.read(encodedIV);
        fis.close();
        //read encryptedCalendar
        fis = new FileInputStream(calendarFilePath);
        byte[] encryptedCalendar = new byte[fis.available()];
        fis.read(encryptedCalendar);
        fis.close();
        //decrypt encryptedCalendar and get Object serialization
        Key key = new SecretKeySpec(encodedKey, KEYGEN_MODE);
        Cipher cipher = Cipher.getInstance(CIPHER_MODE);
        IvParameterSpec ivSpec = new IvParameterSpec(encodedIV);
        cipher.init(Cipher.DECRYPT_MODE, key, ivSpec);
        byte[] encodedCalendar = cipher.doFinal(encryptedCalendar);
        //get object instance from serialization
        ByteArrayInputStream bis = new ByteArrayInputStream(encodedCalendar);
        ObjectInputStream ois = new ObjectInputStream(bis);
        Calendar calendar = (Calendar) ois.readObject();
        return calendar;
    }
}
