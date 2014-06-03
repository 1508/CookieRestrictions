
function createCookie(name, value, days) {
	if (days) {
		var date = new Date();
		date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
		var expires = "; expires=" + date.toGMTString();
	}
	else var expires = "";
	document.cookie = name + "=" + value + expires + "; path=/";
}

function eraseCookie(name) {
	createCookie(name, "", -1);
}

function eraseCookie(name, domain) {
	var value = '';
	var date = new Date();
	date.setTime(date.getTime() + (-1 * 24 * 60 * 60 * 1000));
	var expires = "; expires=" + date.toGMTString(); 
	
	document.cookie = name + "=" + value + expires + ";path=/;domain="+domain;
}

function clearCookies() {
	var cookies = {};
	var cookieNames = new Array();
	var allowCookies = false;
	if (document.cookie && document.cookie != '') {
		var split = document.cookie.split(';');
		for (var i = 0; i < split.length; i++) {
			var nameValue = split[i].split("=");
			var name = nameValue[0];
			var value = nameValue[1];
			if (name == 'allowCookies' && value == 'on') {
				allowCookies = true;
			}
			if (name == 'allowCookies' && value == 'off') {
			    continue;
			}
			    
			cookieNames.push(name);
		}
	}

	if (!allowCookies) {
	    for (var i = 0; i < cookieNames.length; i++) {
			eraseCookie(cookieNames[i]);
			eraseCookie(cookieNames[i], '.' + window.location.hostname);
		}
	}
}

setTimeout('clearCookies()', 1200);