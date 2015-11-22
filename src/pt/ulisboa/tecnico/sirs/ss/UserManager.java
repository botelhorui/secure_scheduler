package pt.ulisboa.tecnico.sirs.ss;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;

/**
 * Created by Rui on 21-11-2015.
 */
public class UserManager implements Serializable {
    private List<User> users = new ArrayList<>();

    public List<User> getUsers() {
        return users;
    }

    public void setUsers(List<User> users) {
        this.users = users;
    }

    @Override
    public String toString() {
        return "UserManager{" +
                "users=" + users +
                '}';
    }
}
