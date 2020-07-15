
$(document).ready( function () {

    $('#school_selector').on('change',function(){
        $('#mycalendar').fullCalendar('rerenderEvents');
    });
    $(document).on("click", ".popover .close" , function(){
        $(this).parents(".popover").popover('hide');
    });

    
     events = [];
     $.ajax({
            type: "GET",
         url: "/Calendar/GetEvents",
         success: function (data) {
             console.log(data);
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
                        subjectClass: v.subjectClass,
                        backgroundColor: v.backgroundColor,
                        textColor: v.textColor,
                        allDay: v.allDay
                    });
                })
            },
            error: function (error) {
                console.log('failed');
            }
        })


    $('#calendar').fullCalendar({
        plugins: [ 'interaction', 'dayGrid', 'timeGrid', 'list' ],
        header: {
            left: 'title prev,next today',
            center: '',
            right: 'pa,fa,cs month,agendaWeek,agendaDay,listWeek'
        },
        locale: 'en-GB',
        timezone: "local",
        nextDayThreshold: "09:00:00",
        defaultView: 'agendaWeek',
        contentHeight: 'auto',
        defaultTimedEventDuration: '01:00:00',
        allDaySlot: false,
        timeFormat: 'HH:mm',
        //defaultDate: '2018-03-07T10:00',
        eventTextColor: '#141437',
        minTime: '09:00:00',
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
                columnFormat:'dddd \nD',
            },
            agendaWeek:{
                columnFormat:'dddd \nD',
                eventLimit: true
            },
            agendaDay:{
                columnFormat:'dddd \nD',
                eventLimit: false
            },
            listWeek:{
                columnFormat:''
            }
        },
        eventClick: function(event, jsEvent, view) {
            //console.log(event);
            $('.popover').popover('hide');
            //editEvent(event);

        },

        eventRender: function(event, element, view) {

            var startTimeEventInfo = moment(event.start).format('HH:mm a');
            var endTimeEventInfo = moment(event.end).format('HH:mm a');
            // console.log(startTimeEventInfo);
            // console.log(endTimeEventInfo);
            var displayEventDate;
            //var eventsColor =
            //    {
            //        'ClassEight': '#d92a2a',
            //        'ClassNine': '#00b8ec',
            //        'ClassTen': '#45b84b',
            //    };


            if(event.allDay == false){
                displayEventDate = startTimeEventInfo + " - " + endTimeEventInfo;
            }else{
                displayEventDate = "All Day";
            }

            //if (event.subjectClass == "ClassEight") {
            //    element.css('background-color', '#fff8f8');
            //    var paColor = '#d92a2a';
            //    element.css('color', paColor);
            //    element.css('border-color', paColor);
            //}
            //if (event.subjectClass == "ClassNine") {
            //    element.css('background-color', '#f1fcff');
            //    var faColor = '#00b8ec';
            //    element.css('color', faColor);
            //    element.css('border-color', faColor);
            //}
            //if (event.subjectClass == "ClassTen") {
            //    var csColor = '#45b84b';
            //    element.css('background-color', '#ebffec');
            //    element.css('color', csColor);
            //    element.css('border-color', csColor);
            //}
            //alert(event.subjectClass);
            element.popover({
                title:    '<span style="background-color: ' + event.backgroundColor + ' !important;" class="popColor"></span><span class="popoverTitleCalendar" style="background-color:#d92a2a; color:#ffffff">' + event.subjectClass + '</span>\n' +
                    '  <a href="#" class="popButton float-right"  onclick="hidethis(this);"><img src="../assets/img/cross-pop.png"></a><a href="#" class="popButton float-right"><img src="../assets/img/edit-pop.png"></a>',
                // title:    '<div class="popoverTitleCalendar" style="background-color:'+ event.backgroundColor +'; color:'+ event.textColor +'">'+ event.title +'</div>',
                content:  '<div class="popoverInfoCalendar">' +
                            '<div class="popTime">' + startTimeEventInfo + '-' + endTimeEventInfo + '</div>' +
                            '<div class="popInstructor"> Instructor ' +
                            '<div>' + event.instructor + '</div>' +
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
                var $popup = $('#' + $(eventShown.target).attr('aria-describedby'));
                // $popup.find('button.cancel').click(function (e) {
                //     $popup.popover('hide');
                // });
                // $popup.find('button.save').click(function (e) {
                //     $popup.popover('hide');
                // });
            });;



            var show_username, show_type = true, show_calendar = true;

            var subjectClass = $('.filterBtnCon .dropdown-item.filterActive').map(function() {
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


    // $('.filter').on('change', function() {
    //     $('#calendar').fullCalendar('rerenderEvents');
    // });

    $('.fc-pa-button, .fc-fa-button, .fc-cs-button ').on('click', function() {
        $('#calendar').fullCalendar('rerenderEvents');
    }).trigger('click');



    /*Filter JS*/
    $('.filterBtnCon .dropdown-item').click(function(event) {
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

} );




function collapseSide(e){
    $('.sidebar').toggleClass('sidebarCollapsed');
    // $('.logo-normal ').toggle();
    // $('.logo-normal ').slideToggle();

}

function hidethis(e){
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
    if(ampm == 'pm'){
        $('.timePM').removeClass('disabledAMPM');
        $('.timeAM').addClass('disabledAMPM');
    }else{
        $('.timePM').addClass('disabledAMPM');
        $('.timeAM').removeClass('disabledAMPM');
    }

    //console.log(hour);
    // hour = 5;
    var timeOfTheDay;
    if(hour > 4 && hour <= 12){
        timeOfTheDay = "Morning";
    }
    if(hour > 12 && hour <= 16){
        timeOfTheDay = "Afternoon";
    }
    if(hour > 16 && hour <= 19){
        timeOfTheDay = "Evening";
    }
    if(hour > 19 || hour < 5){
        timeOfTheDay = "Night";
    }
    //console.log(timeOfTheDay);
    changeTimeImg(timeOfTheDay);

}

function changeTimeImg(time){
    if(time == 'Morning'){
        var img = '../assets/img/morning.png';
    }
    if(time == 'Afternoon'){
        var img = '../assets/img/sun.png';
    }
    if(time == 'Evening'){
        var img = '../assets/img/day-cloud.png';
    }
    if(time == 'Night'){
        var img = '../assets/img/night.png';
    }
    $('.sunSide img').attr('src', img);
    $('.afternoon h1 small span').text(time);
}