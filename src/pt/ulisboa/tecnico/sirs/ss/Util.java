package pt.ulisboa.tecnico.sirs.ss;
/**
 * helper class to check the operating system this Java VM runs in
 *
 * please keep the notes below as a pseudo-license
 *
 * http://stackoverflow.com/questions/228477/how-do-i-programmatically-determine-operating-system-in-java
 * compare to http://svn.terracotta.org/svn/tc/dso/tags/2.6.4/code/base/common/src/com/tc/util/runtime/Os.java
 * http://www.docjar.com/html/api/org/apache/commons/lang/SystemUtils.java.html
 */
import javax.swing.filechooser.FileSystemView;
import java.io.File;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Locale;
public final class Util {

    public static final String SECURE_CALENDAR = "secure.calendar";
    // cached result of OS detection
    protected static OSType detectedOS;


    /**
     * detect the operating system from the os.name System property and cache
     * the result
     *
     * @return - the operating system detected
     */
    public static OSType getOperatingSystemType() {
        if (detectedOS == null) {
            String OS = System.getProperty("os.name", "generic").toLowerCase(Locale.ENGLISH);
            if ((OS.contains("mac")) || (OS.contains("darwin"))) {
                detectedOS = OSType.MacOS;
            } else if (OS.contains("win")) {
                detectedOS = OSType.Windows;
            } else if (OS.contains("nux")) {
                detectedOS = OSType.Linux;
            } else {
                detectedOS = OSType.Other;
            }
        }
        return detectedOS;
    }

    public static Path getStoragePath() {
        OSType operatingSystemType = getOperatingSystemType();
        Path p = null;
        switch (operatingSystemType){
            case Linux:
                p = Paths.get("~", SECURE_CALENDAR);
                break;
            case MacOS:
                p = Paths.get("~", "Documents", SECURE_CALENDAR);
                break;
            case Windows:
                // C://Users/Rui/My Documents
                File docs = FileSystemView.getFileSystemView().getDefaultDirectory();
                Path docsPath = Paths.get(docs.getPath());
                p = docsPath.resolve(SECURE_CALENDAR);
                break;
            case Other:
                throw new RuntimeException(String.format("Unsupported OS: %s", operatingSystemType.name()));

        }
        return p;
    }

    /**
     * types of Operating Systems
     */
    public enum OSType {
        Windows, MacOS, Linux, Other
    }
}
