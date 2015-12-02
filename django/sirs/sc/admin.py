from django.contrib import admin
from .models import Calendar
from .models import CalendarUser
# Register your models here.

admin.site.register(CalendarUser)
admin.site.register(Calendar)