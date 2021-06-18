
$(document).ready(function () {


    $('#school_selector').on('change', function () {
        $('#mycalendar').fullCalendar('rerenderEvents');
    });
    $(document).on("click", ".popover .close", function () {
        $(this).parents(".popover").popover('hide');
    });

    var events = [];
    $.ajax({
        type: "GET",
        url: "/Calendar/GetEvents",
        success: function (data) {
            //console.log(data);
            $.each(data, function (i, v) {
                events.push({
                    _id: v._id,
                    title: v.title,
                    instructor: v.instructor,
                    description: v.description,
                    start: moment(v.start),
                    end: v.end != null ? moment(v.end) : null,
                    type: v.type,
                    calendar: v.calendar,
                    className: v.className,
                    LessonFullName: v.LessonFullName,
                    subjectClass: v.subjectClass,
                    backgroundColor: v.backgroundColor,
                    textColor: v.textColor,
                    allDay: v.allDay,
                });
            });
            RenderCalendar(events);
        },
        error: function (error) {
            console.log('failed');
        }
    })

    //console.log(events);


    // $('.filter').on('change', function() {
    //     $('#calendar').fullCalendar('rerenderEvents');
    // });

    $('.fc-pa-button, .fc-fa-button, .fc-cs-button ').on('click', function () {
        $('#calendar').fullCalendar('rerenderEvents');
    }).trigger('click');



    /*Filter JS*/
    $('.filterBtnCon .dropdown-item').click(function (event) {
        // if ($(this).hasClass('switch-trigger')) {
        if (event.stopPropagation) {
            event.stopPropagation();
        } else if (window.event) {
            window.event.cancelBubble = true;
        }
        // }
        $(this).toggleClass('filterActive');
        $('#calendar').fullCalendar('rerenderEvents');



    }).trigger('click');


    /*show time*/
    showTheTime(); // for the first load
    setInterval(showTheTime, 10000); // update it periodically

});

function RenderCalendar(events) {

    $('#calendar').fullCalendar({
        plugins: ['interaction', 'dayGrid', 'timeGrid', 'list'],
        header: {
            left: 'title prev,next today',
            center: '',
            right: 'pa,fa,cs month,agendaWeek,agendaDay,listWeek'
        },
        locale: 'en-GB',
        timezone: "local",
        nextDayThreshold: "07:30:00",
        defaultView: 'month',
        contentHeight: 'auto',
        defaultTimedEventDuration: '01:00:00',
        allDaySlot: false,
        timeFormat: 'HH:mm',
        //defaultDate: '2018-03-07T10:00',
        eventTextColor: '#141437',
        minTime: '7:30:00',
        maxTime: '22:00:00',
        slotLabelFormat: 'hh:mm a',
        // weekends: true,
        navLinks: true,
        // startTime: '09:00', // a start time (10am in this example)
        // endTime: '21:00', // an end time (6pm in this example)
        eventLimit: true,
        // businessHours: {
        //     // days of week. an array of zero-based day of week integers (0=Sunday)
        //     daysOfWeek: [ 1, 2, 3, 4, 5 ], // Monday - Thursday
        //
        //     startTime: '09:00', // a start time (10am in this example)
        //     endTime: '21:00', // an end time (6pm in this example)
        // },

        // customButtons: {
        //     pa: {
        //         text: 'ClassEight',
        //         click: function() {
        //             $(this).toggleClass('fc-state-active activeClass');
        //         }
        //     },fa: {
        //         text: 'ClassNine',
        //         click: function() {
        //             $(this).toggleClass('fc-state-active activeClass');
        //         }
        //     },cs: {
        //         text: 'ClassTen',
        //         click: function() {
        //             $(this).toggleClass('fc-state-active activeClass');
        //         }
        //     }
        // },


        views: {
            month: {
                columnFormat: 'dddd \nD',
            },
            agendaWeek: {
                columnFormat: 'dddd \nD',
                eventLimit: true
            },
            agendaDay: {
                columnFormat: 'dddd \nD',
                eventLimit: false
            },
            listWeek: {
                columnFormat: ''
            }
        },
        eventClick: function (event, jsEvent, view) {
            //console.log(event);
            $('.popover').popover('hide');
            //editEvent(event);

        },

        eventRender: function (event, element, view) {
            debugger
            var startTimeEventInfo = moment(event.start).format('HH:mm a');
            var endTimeEventInfo = moment(event.end).format('HH:mm a');
            // console.log(startTimeEventInfo);
            // console.log(endTimeEventInfo);
            var displayEventDate;
            var eventsColor =
            {
                'Accounting and Auditing': '#d92a2a',
                'British History': '#2c80c5',
                'Business Administration': '#45b84b',
                'Commerce': '#d92a2a',
                'Constitutional Law': '#2c80c5',
                'Criminology': '#45b84b',
                'Current affairs': '#2c80c5',
                'English': '#e07f9f',
                'English (Precis and Composition)': '#d92a2a',
                'English Essay': '#2c80c5',
                'English Precis and Composition': '#45b84b',
                'Environmental Sciences': '#2c80c5',
                'Every Day Science': '#e07f9f',
                'Gender Studies': '#d92a2a',
                'General science and ability': '#2c80c5',
                'General Knowledge': '#45b84b',
                'Geography': '#2c80c5',
                'History of Indo Pak': '#fcba03',
                'History of USA': '#d92a2a',
                'International Law': '#2c80c5',
                'International Relations': '#d92a2a',
                'Islamiat': '#2c80c5',
                'Islamic studies': '#45b84b',
                'Journalism': '#d92a2a',
                'Mass Communication': '#2c80c5',
                'Math': '#45b84b',
                'Mercantile Law': '#2c80c5',
                'Pakistan affairs': '#e07f9f',
                'Pakistan Studies': '#d92a2a',
                'Physics': '#2c80c5',
                'Political Science': '#45b84b',
                'Public Administration': '#2c80c5',
                'Punjabi': '#e07f9f',
                'Social Work': '#d92a2a',
                'Sociology': '#2c80c5',
                'Solved Past Papers': '#45b84b',
                'Urdu': '#2c80c5',
                'Old': '#2c80c5',
            };


            if (event.allDay == false) {
                displayEventDate = startTimeEventInfo + " - " + endTimeEventInfo;
            } else {
                displayEventDate = "All Day";
            }

            if (event.subjectClass == "Accounting and Auditing") {
                element.css('background-color', 'Red');
                var paColor = '#d92a2a';
                element.css('color', 'black');
                element.css('border-color', paColor);
            }

            if (event.subjectClass == "British History") {
                element.css('background-color', '#ff007f');
                var faColor = '#2c80c5';
                element.css('color', 'black');
                element.css('border-color', faColor);
            }

            if (event.subjectClass == "Business Administration") {
                var csColor = '#45b84b';
                element.css('background-color', 'Green');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Commerce") {
                var csColor = '#d92a2a';
                element.css('background-color', 'yellow');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Constitutional Law") {
                var csColor = '#2c80c5';
                element.css('background-color', 'orange');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Criminology") {
                element.css('background-color', 'pink');
                var paColor = '#45b84b';
                element.css('color', 'black');
                element.css('border-color', paColor);
            }

            if (event.subjectClass == "Current affairs") {
                element.css('background-color', 'gold');
                var faColor = '#2c80c5';
                element.css('color', 'black');
                element.css('border-color', faColor);
            }

            if (event.subjectClass == "English") {
                var csColor = '#e07f9f';
                element.css('background-color', 'red');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "English (Precis and Composition)") {
                var csColor = '#d92a2a';
                element.css('background-color', '#ff007f');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "English Essay") {
                var csColor = '#2c80c5';
                element.css('background-color', 'green');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "English Precis and Composition") {
                element.css('background-color', 'yellow');
                var paColor = '#45b84b';
                element.css('color', 'black');
                element.css('border-color', paColor);
            }

            if (event.subjectClass == "Environmental Sciences") {
                element.css('background-color', 'orange');
                var paColor = '#2c80c5';
                element.css('color', 'black');
                element.css('border-color', paColor);
            }

            if (event.subjectClass == "Every Day Science") {
                element.css('background-color', 'pink');
                var faColor = '#e07f9f';
                element.css('color', 'black');
                element.css('border-color', faColor);
            }

            if (event.subjectClass == "Gender Studies") {
                element.css('background-color', 'gold');
                var paColor = '#d92a2a';
                element.css('color', 'black');
                element.css('border-color', paColor);
            }

            if (event.subjectClass == "General science and ability") {
                element.css('background-color', 'red');
                var faColor = '#2c80c5';
                element.css('color', 'black');
                element.css('border-color', faColor);
            }

            if (event.subjectClass == "General Knowledge") {
                var csColor = '#45b84b';
                element.css('background-color', '#ff007f');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Geography") {
                element.css('background-color', 'green');
                var paColor = '#2c80c5';
                element.css('color', 'black');
                element.css('border-color', paColor);
            }

            if (event.subjectClass == "History of Indo Pak") {
                element.css('background-color', 'yellow');
                var faColor = '#fcba03';
                element.css('color', 'black');
                element.css('border-color', faColor);
            }

            if (event.subjectClass == "History of USA") {
                var csColor = '#d92a2a';
                element.css('background-color', 'orange');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "International Law") {
                element.css('background-color', 'pink');
                var faColor = '#2c80c5';
                element.css('color', 'black');
                element.css('border-color', faColor);
            }

            if (event.subjectClass == "International Relations") {
                var csColor = '#d92a2a';
                element.css('background-color', 'gold');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }
            
            if (event.subjectClass == "Islamiat") {
                var csColor = '#2c80c5';
                element.css('background-color', 'red');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Islamic studies") {
                var csColor = '#45b84b';
                element.css('background-color', '#ff007f');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Journalism") {
                var csColor = '#d92a2a';
                element.css('background-color', 'green');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Mass Communication") {
                var csColor = '#2c80c5';
                element.css('background-color', 'yellow');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Math") {
                var csColor = '#45b84b';
                element.css('background-color', 'orange');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Mercantile Law") {
                var csColor = '#2c80c5';
                element.css('background-color', 'pink');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Pakistan affairs") {
                var csColor = '#e07f9f';
                element.css('background-color', 'gold');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Pakistan Studies") {
                var csColor = '#d92a2a';
                element.css('background-color', 'red');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Physics") {
                var csColor = '#2c80c5';
                element.css('background-color', '#ff007f');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Political Science") {
                var csColor = '#45b84b';
                element.css('background-color', 'green');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            if (event.subjectClass == "Public Administration") {
                var csColor = '#2c80c5';
                element.css('background-color', 'yellow');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }
            if (event.subjectClass == "Punjabi") {
                var csColor = '#e07f9f';
                element.css('background-color', 'orange');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }
            if (event.subjectClass == "Social Work") {
                var csColor = '#d92a2a';
                element.css('background-color', 'pink');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }
            if (event.subjectClass == "Sociology") {
                var csColor = '#2c80c5';
                element.css('background-color', 'gold');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }
            if (event.subjectClass == "Solved Past Papers") {
                var csColor = '#45b84b';
                element.css('background-color', 'red');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }
            if (event.subjectClass == "Urdu") {
                var csColor = '#2c80c5';
                element.css('background-color', '#ff007f');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }
            if (event.subjectClass == "Old") {
                var csColor = '#2c80c5';
                element.css('background-color', 'orange');
                element.css('color', 'black');
                element.css('border-color', csColor);
            }

            //alert(event.subjectClass);
            element.popover({
                title: '<span style="background-color: ' + event.backgroundColor + ' !important;" class="popColor"></span><span class="popoverTitleCalendar" style="background-color:#d92a2a; color:#ffffff">' + event.subjectClass + '</span>\n' +
                    '  <a href="#" class="popButton float-right"  onclick="hidethis(this);"><img src="/Content/assets/img/cross-pop.png"></a><a href="#" class="popButton float-right"></a>',
                // title:    '<div class="popoverTitleCalendar" style="background-color:'+ event.backgroundColor +'; color:'+ event.textColor +'">'+ event.title +'</div>',
                content: '<div class="popoverInfoCalendar">' +
                    '<div class="popTime">' + event.title + ' (' + event.LessonFullName+')</div>' +
                    '<div class="popInstructor"> ' +

                    '</div>' +
                    '</div>',
                delay: {
                    show: "200",
                    hide: "50"
                },
                trigger: 'click',
                placement: 'top',
                html: true,
                container: 'body'
            }).on('shown.bs.popover', function (eventShown) {
                var type = $("#UserType").html();
                //alert(type);

                if (type != "Student") {
                    //alert("Calendar PeekaBoo");
                    var $popup = $('#' + $(eventShown.target).attr('aria-describedby'));
                }
                // $popup.find('button.cancel').click(function (e) {
                //     $popup.popover('hide');
                // });
                // $popup.find('button.save').click(function (e) {
                //     $popup.popover('hide');
                // });
            });;

            var show_username, show_type = true, show_calendar = true;

            var subjectClass = $('.filterBtnCon .dropdown-item.filterActive').map(function () {
                return $(this).text();
            }).get();

            var types = $('#type_filter').val();
            var calendars = $('#calendar_filter').val();

            show_username = subjectClass.indexOf(event.subjectClass) >= 0;

            if (types && types.length > 0) {
                if (types[0] == "all") {
                    show_type = true;
                } else {
                    show_type = types.indexOf(event.type) >= 0;
                }
            }
            if (calendars && calendars.length > 0) {
                if (calendars[0] == "all") {
                    show_calendar = true;
                } else {
                    show_calendar = calendars.indexOf(event.calendar) >= 0;
                }
            }

            return show_username;

        },
        events: events,

        // eventTimeFormat: {
        //     hour: 'numeric',
        //     minute: '2-digit',
        //     omitZeroMinute: true,
        //     meridiem: 'narrow'
        // }
        /*events: [
            {
                title: 'Front-End Conference',
                start: '2020-11-16',
                end: '2020-11-18'
            },
            {
                title: 'Car mechanic',
                start: '2020-11-14T09:00:00',
                end: '2020-11-14T11:00:00',
                color: 'red'
            },
            {
                title: 'Dinner with Mike',
                start: '2020-11-21T19:00:00',
                end: '2020-11-21T22:00:00',
                color: 'blue'
            },
            {
                title: 'Call Meeting',
                start: '2020-11-21T17:00:00',
                end: '2020-11-21T23:00:00',
                color: 'pink'
            },
            {
                title: 'Vacation',
                start: '2020-11-23',
                end: '2020-11-29',
                color: 'grey'
            },
        ]*/
    });

}

function collapseSide(e) {
    $('.sidebar').toggleClass('sidebarCollapsed');
    // $('.logo-normal ').toggle();
    // $('.logo-normal ').slideToggle();

}

function hidethis(e) {
    $(e).closest('.popover').popover('hide');
}


function showTheTime() {
    var hour = parseInt(moment().format('H'));

    var time = moment().format('hh:mm');
    var year = moment().format('YYYY');
    var dayform = moment().format('ddd, DD MMM');
    // console.log(year);
    // console.log(dayform);

    $(".timeTimeCon").text(time);
    $(".timeYear").text(year);
    $(".timeDay").text(dayform);

    var ampm = moment().format('a');
    if (ampm == 'pm') {
        $('.timePM').removeClass('disabledAMPM');
        $('.timeAM').addClass('disabledAMPM');
    } else {
        $('.timePM').addClass('disabledAMPM');
        $('.timeAM').removeClass('disabledAMPM');
    }

    //console.log(hour);
    // hour = 5;
    var timeOfTheDay;
    if (hour > 4 && hour <= 12) {
        timeOfTheDay = "Morning";
    }
    if (hour > 12 && hour <= 16) {
        timeOfTheDay = "Afternoon";
    }
    if (hour > 16 && hour <= 19) {
        timeOfTheDay = "Evening";
    }
    if (hour > 19 || hour < 5) {
        timeOfTheDay = "Night";
    }
    //console.log(timeOfTheDay);
    changeTimeImg(timeOfTheDay);

}

function changeTimeImg(time) {
    if (time == 'Morning') {
        var img = '/Content/assets/img/morning.png';
    }
    if (time == 'Afternoon') {
        var img = '/Content/assets/img/sun.png';
    }
    if (time == 'Evening') {
        var img = '/Content/assets/img/day-cloud.png';
    }
    if (time == 'Night') {
        var img = '/Content/assets/img/night.png';
    }
    $('.sunSide img').attr('src', img);
    $('.afternoon h1 small span').text(time);
}