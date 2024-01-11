var dataTables;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTables = $("#tblData").dataTable({
        "ajax": {
            "url": "/Admin/User/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "email", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "company.name", "width": "15%" },
            { "data": "role", "width": "15%" },
            {
                "data": { id: "id", lockoutEnd: "lockoutEnd" },
                "render": function (data) {
                    var toady = new Date().getTime();
                    var lockOut = new Date(data.lockoutEnd).getTime();
                    if (lockOut > toady) {
                        return `
                                <div class="text-center">
                                <a class="btn btn-danger" onclick=lockUnlock("${data.id}")>Un Lock</a>
                                </div>`;
                    }
                    else {
                        return `
                                <div class="text-center">
                                <a class="btn btn-success" onclick=lockUnlock("${data.id}")>Lock</a>
                                </div>`;
                    }
                }
            }
        ]
    })
}

function lockUnlock(id) {

    $.ajax({
        url: "/Admin/User/LockUnlock",
        type: "POST",
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {

            if (data.success) {
                toastr.success(data.message);
                $("#tblData").DataTable().ajax.reload();
            }
            else {
                toastr.error(data.message);
            }
        }
    })
}