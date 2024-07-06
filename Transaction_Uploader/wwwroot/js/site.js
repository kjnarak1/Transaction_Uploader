var transaction_list;
var row_per_page = 100;

$(document).ready(function () {
    $('#searchButton').click(function () {
        document.getElementById("loading").classList.remove("hidden");
        document.getElementsByClassName("display")[0].classList.add("hidden");
        $.ajax({
            url: '/api/Transactions',
            method: 'GET',
            data: {
                currency: $('#currency').val(),
                startDate: $('#start_date').val(),
                endDate: $('#end_date').val(),
                status: $('#status').val()
            },
            statusCode: {
                200: function (response) {
                    document.getElementById("loading").classList.add("hidden");
                    transaction_list = response;
                    load_pagination();
                    load_page(1);
                    document.getElementsByClassName("display")[0].classList.remove("hidden");
                },
                404: function () {
                    document.getElementById("loading").classList.add("hidden");
                    alert('No transactions found for the given criteria.');
                }
            },
            error: function () {
                document.getElementById("loading").classList.add("hidden");
                alert('Failed to export data.');
            }
        });
    });
});

function load_pagination() {
    var num_page = transaction_list.length / row_per_page;
    if (transaction_list.length % row_per_page != 0) {
        num_page++;
    }
    var pagination = document.getElementsByClassName("pagination")[0];
    while (pagination.firstChild) {
        pagination.removeChild(pagination.firstChild);
    }
    for (var i = 1; i <= num_page; i++) {
        (function (index) {
            var span = document.createElement("span");
            span.innerText = index;
            if (i == 1) {
                span.classList.add("active");
            }
            span.onclick = function () {
                load_page(index);
                var spans = document.querySelectorAll('.pagination span');
                spans.forEach(function (span) {
                    span.classList.remove('active');
                });
                span.classList.add("active");
            };
            pagination.appendChild(span);
        })(i);
    }
}
function mapStatus(transaction) {
    let newStatus = '';
    if (transaction.id.startsWith('Invoice')) {
        switch (transaction.status) {
            case 'A':
                newStatus = 'Approved';
                break;
            case 'R':
                newStatus = 'Failed';
                break;
            case 'D':
                newStatus = 'Finished';
                break;
            default:
                newStatus = transaction.status;
        }
    } else if (transaction.id.startsWith('Inv')) {
        switch (transaction.status) {
            case 'A':
                newStatus = 'Approved';
                break;
            case 'R':
                newStatus = 'Rejected';
                break;
            case 'D':
                newStatus = 'Done';
                break;
            default:
                newStatus = transaction.status;
        }
        return newStatus;
    }
}

function load_page(page) {
    var table = document.getElementsByTagName("table")[0].getElementsByTagName("tbody")[0];
    while (table.firstChild) {
        table.removeChild(table.firstChild);
    }

    for (var i = (page - 1) * row_per_page; i < row_per_page * page && i < transaction_list.length; i++) {
        var transaction = transaction_list[i];

        var tr = document.createElement("tr");
        var td0 = document.createElement("td");
        td0.innerHTML = transaction.id;
        tr.appendChild(td0);

        var td1 = document.createElement("td");
        td1.innerHTML = transaction.payment;
        tr.appendChild(td1);

        var td2 = document.createElement("td");
        td2.classList.add("status");
        var span2 = document.createElement("span");
        span2.innerHTML = mapStatus(transaction);
        if (transaction.status == "A") {
            span2.classList.add("approve");
        } else if (transaction.status == "R") {
            span2.classList.add("reject");
        } else {
            span2.classList.add("done");
        }
        td2.appendChild(span2);
        tr.appendChild(td2);

        table.appendChild(tr);
    }
}

function openModal(modalId) {
    var currentDate = new Date();
    var formattedDate = currentDate.toISOString().slice(0, 10);
    var modal = document.getElementById(modalId);
    var span = modal.querySelector(".close");
    modal.style.display = "block";
    span.onclick = function () {
        modal.style.animation = "fadeOut 0.3s ease forwards";
        setTimeout(function () {
            modal.style.display = "none";
            modal.style.animation = "";
        }, 300);
    }
    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.animation = "fadeOut 0.3s ease forwards";
            setTimeout(function () {
                modal.style.display = "none";
                modal.style.animation = "";
            }, 300);
        }
    }
}

document.getElementById('uploadForm').addEventListener('submit', async function (event) {
    event.preventDefault();

    const fileInput = document.getElementById('fileInput');
    if (fileInput.files.length === 0) {
        alert('Please select a file to upload.');
        return;
    }

    const file = fileInput.files[0];
    const allowedExtensions = ['csv', 'xml'];
    const fileExtension = file.name.split('.').pop().toLowerCase();

    if (!allowedExtensions.includes(fileExtension)) {
        alert('Please upload a CSV or XML file.');
        return;
    }

    const formData = new FormData();
    formData.append('file', file);

    try {
        const response = await fetch('/api/Upload', {
            method: 'POST',
            body: formData
        });
        console.log(response);
        const result = await response.json();
        const responseMessage = document.getElementById('responseMessage');
        if (response.ok) {
            responseMessage.textContent = `Success`;
            responseMessage.style.color = 'green';
        } else {
            responseMessage.textContent = `Error: ${result.message}`;
            responseMessage.style.color = 'red';
        }
    } catch (error) {
        const responseMessage = document.getElementById('responseMessage');
        responseMessage.textContent = error;
        responseMessage.style.color = 'red';
    }
});