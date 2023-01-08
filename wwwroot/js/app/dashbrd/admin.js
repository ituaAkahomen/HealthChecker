

$.ajax({
    method: "POST",
    url: "/home/widget",
    data: { duration: 0 }
})
.done(function (result) {
    $("#enrollees").html(result.enrollees);
    $("#sall").html(result.sall);
    $("#signeds").html(result.signeds);
    $("#excluded").html(result.excluded);
    $("#providers").html(result.providers);
    $("#locations").html(result.locations);
});

$('#dataps').DataTable({
    "lengthMenu": [[50, 100], ["50", "100"]],
    "dom": 'Bfrtip',
    "buttons": [
        'copyHtml5',
        'excelHtml5',
        'csvHtml5',
        'pdfHtml5'
    ],
    "searchHighlight": true,
    "serverSide": true,
    "ajax": {
        "type": "POST",
        "url": '/Home/PendingSignups',
        "contentType": 'application/json; charset=utf-8',
        'data': function (data) { return data = JSON.stringify(data); }
    },
    "processing": true,
    "language": {
        processing: '<i class="fa fa-circle-o-notch fa-spin"></i><span class="sr-only">Loading...</span>'
    },
    "paging": true,
    "deferRender": true,
    "columns": [
        {
            data: 'sn',
            sortable: false,
            searchable: false
        },
        {
            data: 'empid'
        },
        {
            data: 'names'
        },
        {
            data: 'enrollid'
        },
        {
            data: 'gender'
        },
        {
            data: 'lastupdated'
        },
        {
            searchable: false,
            sortable: false,
            data: "id",
            "render": function (data, type, full, meta) {
                return '<a class="btn btn-info btn-sm" title="Details" href="/enrollees/details/' + data + '">Details</a>';
            }
        }
    ]
});

