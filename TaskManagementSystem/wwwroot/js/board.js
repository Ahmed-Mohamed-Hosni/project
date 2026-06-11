document.addEventListener('DOMContentLoaded', () => {
    // Add Card UI Logic
    document.querySelectorAll('.add-card-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const listId = e.target.dataset.listId;
            e.target.classList.add('d-none');
            document.getElementById(`form-${listId}`).classList.remove('d-none');
            document.getElementById(`input-${listId}`).focus();
        });
    });

    document.querySelectorAll('.add-card-cancel').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const listId = e.target.dataset.listId;
            document.getElementById(`form-${listId}`).classList.add('d-none');
            document.querySelector(`.add-card-btn[data-list-id="${listId}"]`).classList.remove('d-none');
            document.getElementById(`input-${listId}`).value = '';
        });
    });

    document.querySelectorAll('.add-card-submit').forEach(btn => {
        btn.addEventListener('click', async (e) => {
            const listId = e.target.dataset.listId;
            const input = document.getElementById(`input-${listId}`);
            const title = input.value.trim();
            
            if (!title) return;

            try {
                const response = await fetch('/Board/CreateCard', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                    },
                    body: `listId=${listId}&title=${encodeURIComponent(title)}`
                });

                if (response.ok) {
                    const card = await response.json();
                    
                    // Add card to UI
                    const container = document.querySelector(`.cards-container[data-list-id="${listId}"]`);
                    const cardEl = document.createElement('div');
                    cardEl.className = 'glass task-card';
                    cardEl.draggable = true;
                    cardEl.dataset.cardId = card.id;
                    cardEl.innerHTML = `<div class="fw-semibold">${card.title}</div>`;
                    
                    // Reattach drag events to new card
                    attachDragEvents(cardEl);
                    
                    container.appendChild(cardEl);
                    
                    // Reset form
                    input.value = '';
                    document.getElementById(`form-${listId}`).classList.add('d-none');
                    document.querySelector(`.add-card-btn[data-list-id="${listId}"]`).classList.remove('d-none');
                }
            } catch (err) {
                console.error("Error creating card:", err);
            }
        });
    });

    // Drag and Drop Logic
    const containers = document.querySelectorAll('.cards-container');
    
    function attachDragEvents(card) {
        card.addEventListener('dragstart', () => {
            card.classList.add('dragging');
        });
        
        card.addEventListener('dragend', async () => {
            card.classList.remove('dragging');
            
            // Save new position
            const newContainer = card.closest('.cards-container');
            const newListId = newContainer.dataset.listId;
            const cardId = card.dataset.cardId;
            
            const cardsInList = Array.from(newContainer.querySelectorAll('.task-card'));
            const newOrder = cardsInList.indexOf(card) + 1;

            try {
                await fetch('/Board/MoveCard', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                    },
                    body: `cardId=${cardId}&newListId=${newListId}&newOrder=${newOrder}`
                });
            } catch (err) {
                console.error("Error moving card:", err);
            }
        });
    }

    document.querySelectorAll('.task-card').forEach(attachDragEvents);

    containers.forEach(container => {
        container.addEventListener('dragover', e => {
            e.preventDefault();
            const afterElement = getDragAfterElement(container, e.clientY);
            const draggable = document.querySelector('.dragging');
            if (afterElement == null) {
                container.appendChild(draggable);
            } else {
                container.insertBefore(draggable, afterElement);
            }
        });
    });

    function getDragAfterElement(container, y) {
        const draggableElements = [...container.querySelectorAll('.task-card:not(.dragging)')];

        return draggableElements.reduce((closest, child) => {
            const box = child.getBoundingClientRect();
            const offset = y - box.top - box.height / 2;
            if (offset < 0 && offset > closest.offset) {
                return { offset: offset, element: child }
            } else {
                return closest;
            }
        }, { offset: Number.NEGATIVE_INFINITY }).element;
    }
});
