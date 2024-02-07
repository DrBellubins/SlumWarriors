#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

// NOTE: Add here your custom variables
uniform sampler2D maskTexture;

void main()
{
	vec4 outColor = vec4(0, 0, 0, 0);
	
	vec4 uiCol = texture(texture0, fragTexCoord);
	vec4 uiMaskColor = texture(maskTexture, fragTexCoord);
	
	if (ceil(uiMaskColor.a) == 1)
		outColor = uiCol;
	else
		outColor = vec4(0, 0, 0, 0);

    finalColor = outColor;
}