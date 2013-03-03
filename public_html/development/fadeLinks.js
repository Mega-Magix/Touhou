this.fadeLinks = function() {	
	
	var selector = "#content a"; //modify this line to set the selectors
	var speed = "normal" // adjust the fading speed ("slow", "normal", "fast")
	
	var bgcolor = "#fff"; 	// unfortunately we have to set bg color because of that freakin' IE *!$%#!!?!?%$! 
							//please use the same background color in your links as it is in your document. 
							
	$(selector).each(function(){ 
		$(this).css("position","relative");
		var html = $(this).html();
		$(this).html("<span class="one">"+ html +"</span>");
		$(this).append("<span class="two">"+ html +"</span<");		
		if($.browser.msie){
			$("span.one",$(this)).css("background",bgcolor);
			$("span.two",$(this)).css("background",bgcolor);	
			$("span.one",$(this)).css("opacity",1);			
		};
		$("span.two",$(this)).css("opacity",0);		
		$("span.two",$(this)).css("position","absolute");		
		$("span.two",$(this)).css("top","0");
		$("span.two",$(this)).css("left","0");		
		$(this).hover(
			function(){
				$("span.one",this).fadeTo(speed, 0);				
				$("span.two",this).fadeTo(speed, 1);
			},
			function(){
				$("span.one",this).fadeTo(speed, 1);	
				$("span.two",this).fadeTo(speed, 0);
			}			
		)
	});
};