package sdtrabalho.validation.api.security;

import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.stream.Collectors;

import javax.crypto.spec.SecretKeySpec;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.core.convert.converter.Converter;
import org.springframework.security.authentication.AbstractAuthenticationToken;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.config.annotation.method.configuration.EnableMethodSecurity;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.core.GrantedAuthority;
import org.springframework.security.core.authority.SimpleGrantedAuthority;
import org.springframework.security.oauth2.jwt.Jwt;
import org.springframework.security.oauth2.jwt.JwtDecoder;
import org.springframework.security.oauth2.jwt.NimbusJwtDecoder;
import org.springframework.security.web.SecurityFilterChain;

@Configuration
@EnableMethodSecurity
public class SecurityConfig {

  @Value("${jwt.secret}")
  private String jwtSecret;

  @Bean
  JwtDecoder jwtDecoder() {
    var keyBytes = jwtSecret.getBytes(StandardCharsets.UTF_8);
    var key = new SecretKeySpec(keyBytes, "HmacSHA256");
    return NimbusJwtDecoder.withSecretKey(key).build();
  }

  @Bean
  SecurityFilterChain filterChain(HttpSecurity http) throws Exception {
    http
      .csrf(csrf -> csrf.disable())
      .authorizeHttpRequests(auth -> auth
        .requestMatchers("/health", "/swagger-ui/**", "/v3/api-docs/**").permitAll()
        .anyRequest().authenticated()
      )
      .oauth2ResourceServer(oauth -> oauth
        .jwt(jwt -> jwt.jwtAuthenticationConverter(jwtAuthConverter()))
      );

    return http.build();
  }

  @Bean
  Converter<Jwt, AbstractAuthenticationToken> jwtAuthConverter() {
    return jwt -> {
      var authorities = extractAuthorities(jwt);
      return new UsernamePasswordAuthenticationToken(
          jwt.getSubject(),
          null,
          authorities
      );
    };
  }

  private Collection<GrantedAuthority> extractAuthorities(Jwt jwt) {
    List<String> roles = new ArrayList<>();

    Object roleClaim = firstNonNull(
      jwt.getClaims().get("role")
    );

    if (roleClaim instanceof String s) {
      roles.add(s);
    } else if (roleClaim instanceof Collection<?> c) {
      for (var r : c) roles.add(String.valueOf(r));
    }

    return roles.stream()
      .filter(r -> r != null && !r.isBlank())
      .map(r -> "ROLE_" + r)
      .map(SimpleGrantedAuthority::new)
      .collect(Collectors.toSet());
  }

  private Object firstNonNull(Object... values) {
    for (var v : values) if (v != null) return v;
    return null;
  }
}
