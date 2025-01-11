function allowDrop(event) {
  event.preventDefault();
}

function drag(event) {
  event.dataTransfer.setData("text", event.target.getAttribute("data-id"));
}

function drop(event) {
  event.preventDefault();
  const taskId = event.dataTransfer.getData("text");
  const newStatus = event.target
    .closest(".dropzone")
    .getAttribute("data-status");

  // Mută task-ul vizual în noua coloană
  const taskCard = document.querySelector(`#task-card-${taskId}`);
  event.target.closest(".dropzone").appendChild(taskCard);

  // Trimite modificarea către server prin AJAX
  updateTaskStatus(taskId, newStatus);
}

function updateTaskStatus(taskId, newStatus) {
  fetch(`/Tasks/UpdateStatus`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: document.querySelector(
        'input[name="__RequestVerificationToken"]'
      ).value,
    },
    body: JSON.stringify({ id: taskId, status: newStatus }),
  })
    .then((response) => {
      if (!response.ok) {
        console.error("Failed to update task status.");
      }
    })
    .catch((error) => console.error("Error:", error));
}
