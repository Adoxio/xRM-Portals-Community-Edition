/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

$(function () {
  $("div.voting-container > div > a.vote").click(function () {
    $(this).parent().parent("div.voting-container").block({ message: null, overlayCSS: { opacity: .3} });
  });
  $("div.modal-vote > div > div > a.vote").click(function () {
    $(this).parent().parent().parent().prev("div.voting-container").block({ message: null, overlayCSS: { opacity: .3} });
  });
});

function setVote(url, voteValue, Id) {
	shell.ajaxSafePost({
		type: "POST",
		contentType: "application/json",
		url: url,
		global: false,
		success: function (result) {
			$("#vote-modal-" + Id).modal('toggle');
			$("#vote-status-" + Id).html(result);
			updateUserVoteCount(voteValue);
		}
	});
}

function updateUserVoteCount(voteValue) {
  var previousVotesLeft = $('#votes-left .badge');
  if (previousVotesLeft.length) {
    previousVotesLeft.fadeOut("slow", function () {
      var votesLeft = previousVotesLeft.text() - voteValue;
      previousVotesLeft.text(votesLeft > 0 ? votesLeft : 0);
      previousVotesLeft.fadeIn("slow");
    });
  }
}
