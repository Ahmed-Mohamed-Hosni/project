"use strict";

let connection = null;
let currentRoomId = null;
let currentUserId = null;
let currentUserName = null;
let typingTimeout = null;
let isTyping = false;

// Audio notification for incoming messages
const notificationSound = new Audio("https://assets.mixkit.co/active_storage/sfx/2357/2357-500.wav");
notificationSound.volume = 0.3;

document.addEventListener("DOMContentLoaded", function () {
    // Read current user info from hidden inputs in view
    currentUserId = parseInt(document.getElementById("currentUserId").value);
    currentUserName = document.getElementById("currentUserName").value;

    initSignalR();
    loadRooms();
    setupEventListeners();
});

// Initialize SignalR Connection
function initSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    // Event: Receive message
    connection.on("ReceiveMessage", function (msg) {
        if (msg.chatRoomId === currentRoomId) {
            appendMessage(msg);
            scrollToBottom();
        } else {
            // Play notification sound for background chats
            notificationSound.play().catch(e => console.log("Audio play deferred until user interaction"));
        }
        
        // Update the last message preview in the sidebar
        updateSidebarMessagePreview(msg);
    });

    // Event: Online/Offline status changes
    connection.on("UserStatusChanged", function (userId, isOnline) {
        // Update status dot in chat list
        const statusDot = document.querySelector(`.room-item[data-user-id="${userId}"] .online-indicator`);
        if (statusDot) {
            if (isOnline) {
                statusDot.classList.remove("offline");
                statusDot.classList.add("pulse");
                const preview = document.querySelector(`.room-item[data-user-id="${userId}"] .room-preview`);
                if (preview && preview.innerText === "Offline") {
                    preview.innerText = "Online";
                }
            } else {
                statusDot.classList.add("offline");
                statusDot.classList.remove("pulse");
                const preview = document.querySelector(`.room-item[data-user-id="${userId}"] .room-preview`);
                if (preview) {
                    preview.innerText = "Offline";
                }
            }
        }

        // Update status in header if chatting with this user
        const activeChat = document.getElementById("activeChatHeader");
        if (activeChat && activeChat.getAttribute("data-user-id") === userId.toString()) {
            const statusEl = document.getElementById("activeChatStatus");
            if (statusEl) {
                statusEl.innerText = isOnline ? "Online" : "Offline";
            }
        }
    });

    // Event: Typing State Updates
    connection.on("ReceiveTypingState", function (chatRoomId, userId, username, isUserTyping) {
        if (chatRoomId === currentRoomId && userId !== currentUserId) {
            const typingIndicator = document.getElementById("typingIndicator");
            const typingText = document.getElementById("typingUserText");
            
            if (isUserTyping) {
                typingText.innerText = `${username} is typing...`;
                typingIndicator.style.display = "flex";
                scrollToBottom();
            } else {
                typingIndicator.style.display = "none";
            }
        }
    });

    // Start connection
    connection.start().then(function () {
        console.log("Connected to ChatHub successfully!");
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

// Load Chat Rooms list
function loadRooms(selectRoomId = null) {
    fetch("/Chat/GetRooms")
        .then(res => res.json())
        .then(rooms => {
            const directList = document.getElementById("directChatList");
            const groupList = document.getElementById("groupChatList");
            
            directList.innerHTML = "";
            groupList.innerHTML = "";

            if (rooms.length === 0) {
                directList.innerHTML = `<div class="sidebar-info-text">No active conversations. Search a user to start chatting!</div>`;
                groupList.innerHTML = `<div class="sidebar-info-text">No groups joined yet. Create one above!</div>`;
                return;
            }

            rooms.forEach(room => {
                const isDM = !room.isGroup;
                const activeClass = room.id === currentRoomId ? "active" : "";
                const onlineClass = room.isOnline ? "" : "offline";
                const pulseClass = room.isOnline ? "pulse" : "";

                const roomHtml = `
                    <div class="room-item ${activeClass}" data-room-id="${room.id}" onclick="switchRoom(${room.id})">
                        <div class="room-avatar-container">
                            <img class="room-avatar" src="${room.avatar}" alt="${room.name}" />
                            ${isDM ? `<div class="online-indicator ${onlineClass} ${pulseClass}"></div>` : ''}
                        </div>
                        <div class="room-info">
                            <div class="room-info-header">
                                <h4 class="room-name">${room.name}</h4>
                                <span class="room-time" data-time-room="${room.id}">${room.lastMessageTime}</span>
                            </div>
                            <p class="room-preview" data-preview-room="${room.id}">${room.lastMessage}</p>
                        </div>
                    </div>
                `;

                if (room.isGroup) {
                    groupList.innerHTML += roomHtml;
                } else {
                    directList.innerHTML += roomHtml;
                }
            });

            // Map data-user-ids for DMs
            bindDmUserIds(rooms);

            if (selectRoomId) {
                switchRoom(selectRoomId);
            }
        });
}

// Map userIds to DMs in UI to toggle green lights
function bindDmUserIds(rooms) {
    rooms.forEach(room => {
        if (!room.isGroup) {
            // Fetch room members to bind user id
            fetch(`/Chat/GetMessages?roomId=${room.id}`)
                .then(res => res.json())
                .then(messages => {
                    // Find sender who is not me
                    const otherSender = messages.find(m => m.senderId !== currentUserId);
                    if (otherSender) {
                        const el = document.querySelector(`.room-item[data-room-id="${room.id}"]`);
                        if (el) {
                            el.setAttribute("data-user-id", otherSender.senderId);
                            // Set initial status dot based on room.isOnline
                            const dot = el.querySelector(".online-indicator");
                            if (dot) {
                                if (room.isOnline) {
                                    dot.classList.remove("offline");
                                    dot.classList.add("pulse");
                                } else {
                                    dot.classList.add("offline");
                                    dot.classList.remove("pulse");
                                }
                            }
                        }
                    }
                });
        }
    });
}

// Switch Chat Room
function switchRoom(roomId) {
    if (currentRoomId === roomId) return;
    
    // Clear typing state in previous room
    if (currentRoomId && isTyping) {
        sendTypingState(false);
    }

    currentRoomId = roomId;
    isTyping = false;

    // Highlight selected room in sidebar
    document.querySelectorAll(".room-item").forEach(el => {
        el.classList.remove("active");
    });
    const selectedEl = document.querySelector(`.room-item[data-room-id="${roomId}"]`);
    if (selectedEl) {
        selectedEl.classList.add("active");
    }

    // Hide empty state and show active chat workspace
    document.getElementById("emptyState").style.display = "none";
    document.getElementById("activeChatArea").style.display = "flex";

    // Setup Header details
    const roomName = selectedEl.querySelector(".room-name").innerText;
    const roomAvatar = selectedEl.querySelector(".room-avatar").src;
    
    document.getElementById("activeChatName").innerText = roomName;
    document.getElementById("activeChatAvatar").src = roomAvatar;

    const statusEl = document.getElementById("activeChatStatus");
    const indicator = selectedEl.querySelector(".online-indicator");
    
    if (indicator) {
        const isOnline = !indicator.classList.contains("offline");
        statusEl.innerText = isOnline ? "Online" : "Offline";
        document.getElementById("activeChatHeader").setAttribute("data-user-id", selectedEl.getAttribute("data-user-id") || "");
    } else {
        statusEl.innerText = "Group Chat";
        document.getElementById("activeChatHeader").removeAttribute("data-user-id");
    }

    // Load message history
    loadMessages(roomId);

    // Notify SignalR hub to join room group
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("JoinRoom", roomId).catch(err => console.error(err));
    }

    // Close sidebar toggle on mobile
    const sidebar = document.getElementById("chatSidebar");
    if (sidebar) {
        sidebar.classList.add("collapsed");
    }
}

// Load messages for specific room
function loadMessages(roomId) {
    const container = document.getElementById("messagesList");
    container.innerHTML = `<div class="sidebar-info-text"><i class="fa-solid fa-spinner fa-spin"></i> Loading messages...</div>`;

    fetch(`/Chat/GetMessages?roomId=${roomId}`)
        .then(res => res.json())
        .then(messages => {
            container.innerHTML = "";
            
            if (messages.length === 0) {
                container.innerHTML = `<div class="sidebar-info-text">No messages yet. Send a message to start the conversation!</div>`;
                return;
            }

            messages.forEach(msg => {
                appendMessage(msg);
            });

            scrollToBottom();
        });
}

// Append single message to workspace
function appendMessage(msg) {
    const container = document.getElementById("messagesList");
    
    // Remove "no messages yet" text if present
    const infoText = container.querySelector(".sidebar-info-text");
    if (infoText) infoText.remove();

    const isOutgoing = msg.senderId === currentUserId;
    const msgWrapperClass = isOutgoing ? "outgoing" : "incoming";

    const msgHtml = `
        <div class="message-wrapper ${msgWrapperClass}">
            ${!isOutgoing ? `<img class="msg-sender-avatar" src="${msg.senderAvatar}" alt="${msg.senderName}" />` : ''}
            <div class="message-bubble-content">
                ${!isOutgoing ? `<span class="msg-sender-name">${msg.senderName}</span>` : ''}
                <div class="message-bubble">
                    <p class="message-text">${escapeHtml(msg.content)}</p>
                </div>
                <span class="message-time">${msg.formattedTime}</span>
            </div>
        </div>
    `;

    container.innerHTML += msgHtml;
}

// Send Message
function sendMessage() {
    const input = document.getElementById("messageInput");
    const content = input.value.trim();
    
    if (!content || !currentRoomId) return;

    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("SendMessage", currentRoomId, content)
            .then(() => {
                input.value = "";
                input.style.height = "auto";
                sendTypingState(false);
            })
            .catch(err => console.error(err));
    } else {
        alert("SignalR Connection is lost. Reconnecting...");
    }
}

// Typing state change sender
function sendTypingState(typing) {
    if (!currentRoomId || !connection || connection.state !== signalR.HubConnectionState.Connected) return;

    isTyping = typing;
    connection.invoke("SendTypingState", currentRoomId, typing)
        .catch(err => console.error(err));
}

// Update last message in sidebar
function updateSidebarMessagePreview(msg) {
    const preview = document.querySelector(`.room-item[data-room-id="${msg.chatRoomId}"] .room-preview`);
    const time = document.querySelector(`.room-item[data-room-id="${msg.chatRoomId}"] .room-time`);
    
    if (preview) {
        preview.innerText = msg.content;
        preview.style.fontWeight = "600";
        preview.style.color = "var(--text-main)";
    }
    if (time) {
        time.innerText = msg.formattedTime;
    }
}

// Setup JS Event Listeners
function setupEventListeners() {
    // Send button
    document.getElementById("btnSendMessage").addEventListener("click", sendMessage);

    // Message Input typing
    const msgInput = document.getElementById("messageInput");
    msgInput.addEventListener("keydown", function (e) {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

    msgInput.addEventListener("input", function () {
        // Auto-expand textarea height
        this.style.height = "auto";
        this.style.height = (this.scrollHeight) + "px";

        // Handle typing state
        if (!isTyping) {
            sendTypingState(true);
        }

        clearTimeout(typingTimeout);
        typingTimeout = setTimeout(function () {
            sendTypingState(false);
        }, 1500);
    });

    // Toggle sidebar on mobile
    const toggleBtn = document.getElementById("mobileSidebarToggle");
    if (toggleBtn) {
        toggleBtn.addEventListener("click", function () {
            document.getElementById("chatSidebar").classList.remove("collapsed");
        });
    }

    const closeSidebarBtn = document.getElementById("closeSidebarBtn");
    if (closeSidebarBtn) {
        closeSidebarBtn.addEventListener("click", function () {
            document.getElementById("chatSidebar").classList.add("collapsed");
        });
    }

    // Search Users input handler
    const searchInput = document.getElementById("sidebarSearchInput");
    const searchDropdown = document.getElementById("searchResultsDropdown");

    searchInput.addEventListener("input", function () {
        const query = this.value.trim();
        if (query.length < 2) {
            searchDropdown.style.display = "none";
            return;
        }

        fetch(`/Chat/SearchUsers?query=${query}`)
            .then(res => res.json())
            .then(users => {
                searchDropdown.innerHTML = "";
                if (users.length === 0) {
                    searchDropdown.innerHTML = `<div style="padding:15px;text-align:center;font-size:0.85rem;color:var(--text-muted);">No users found.</div>`;
                    searchDropdown.style.display = "block";
                    return;
                }

                users.forEach(user => {
                    searchDropdown.innerHTML += `
                        <div class="search-result-item" onclick="startDirectChat(${user.id})">
                            <img src="${user.avatarUrl}" alt="${user.username}" />
                            <div class="search-result-details">
                                <h5>${user.username}</h5>
                                <p>${user.isOnline ? "Online" : "Offline"}</p>
                            </div>
                            <div class="search-result-add">
                                <i class="fa-solid fa-message"></i>
                            </div>
                        </div>
                    `;
                });
                searchDropdown.style.display = "block";
            });
    });

    // Close search dropdown when clicking outside
    document.addEventListener("click", function (e) {
        if (!searchInput.contains(e.target) && !searchDropdown.contains(e.target)) {
            searchDropdown.style.display = "none";
        }
    });

    // Tabs switching logic
    document.getElementById("tabDirect").addEventListener("click", function () {
        switchTab("direct", this);
    });
    document.getElementById("tabGroup").addEventListener("click", function () {
        switchTab("group", this);
    });
}

function switchTab(type, button) {
    document.querySelectorAll(".tab-btn").forEach(btn => btn.classList.remove("active"));
    button.classList.add("active");

    if (type === "direct") {
        document.getElementById("directChatList").style.display = "block";
        document.getElementById("groupChatList").style.display = "none";
    } else {
        document.getElementById("directChatList").style.display = "none";
        document.getElementById("groupChatList").style.display = "block";
    }
}

// AJAX: Start DM chat
function startDirectChat(userId) {
    fetch(`/Chat/StartPrivateChat?otherUserId=${userId}`, { method: "POST" })
        .then(res => res.json())
        .then(result => {
            document.getElementById("sidebarSearchInput").value = "";
            document.getElementById("searchResultsDropdown").style.display = "none";
            loadRooms(result.roomId);
        });
}

// Modal open/close actions
function openModal(modalId) {
    document.getElementById(modalId).classList.add("open");
}

// Modal close action
function closeModal(modalId) {
    document.getElementById(modalId).classList.remove("open");
}

// AJAX: Create Group Room
function submitCreateGroup() {
    const name = document.getElementById("groupNameInput").value.trim();
    const desc = document.getElementById("groupDescInput").value.trim();

    if (!name) {
        alert("Group name is required!");
        return;
    }

    fetch(`/Chat/CreateGroup?name=${name}&description=${desc}`, { method: "POST" })
        .then(res => res.json())
        .then(result => {
            closeModal("createGroupModal");
            document.getElementById("groupNameInput").value = "";
            document.getElementById("groupDescInput").value = "";
            loadRooms(result.roomId);
        });
}

// Utility Helpers
function scrollToBottom() {
    const container = document.getElementById("messagesList");
    container.scrollTop = container.scrollHeight;
}

function escapeHtml(str) {
    return str.replace(/&/g, "&amp;")
              .replace(/</g, "&lt;")
              .replace(/>/g, "&gt;")
              .replace(/"/g, "&quot;")
              .replace(/'/g, "&#039;");
}
