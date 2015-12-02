from django.db import models


# Create your models here.
class CalendarUser(models.Model):
    username = models.CharField(max_length=200, unique=True)
    encryptedPrivateRSAKey = models.CharField(max_length=3000)
    publicRSAKey = models.CharField(max_length=3000)


class Calendar(models.Model):
    name = models.CharField(max_length=200)
    owner = models.ForeignKey(CalendarUser)
    encryptedFEK = models.CharField(max_length=3000)
    encryptedCalendarData = models.CharField(max_length=10000)
